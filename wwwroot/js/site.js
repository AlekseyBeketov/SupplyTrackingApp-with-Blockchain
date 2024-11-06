// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', function () {
    let productContainer = document.getElementById('productContainer');
    let addedProductsContainer = document.getElementById('addedProductsContainer');
    let productRow = productContainer.querySelector('.productRow');
    let productCounter = 0;
    let lastSelectedProductId = null;
    let addedProducts = [];  // Массив для сохраненных товаров

    const accordionState = localStorage.getItem('accordionOpen');
    const collapseElement = document.getElementById('collapseOne');
    if (accordionState === 'true') {
        collapseElement.classList.add('show'); // Открываем аккордеон
    }

    // Слушаем изменения состояния аккордеона и сохраняем его
    collapseElement.addEventListener('show.bs.collapse', () => {
        localStorage.setItem('accordionOpen', 'true');
    });

    collapseElement.addEventListener('hide.bs.collapse', () => {
        localStorage.setItem('accordionOpen', 'false');
    });

    // Обработчик для селектора производителя
    document.getElementById('vendorSelect').addEventListener('change', function () {
        localStorage.setItem('accordionOpen', 'true'); // Сохраняем состояние как открытое перед отправкой формы
        document.forms[0].submit();
    });

    productContainer.addEventListener('click', function (event) {
        if (event.target.classList.contains('addProduct')) {
            let productSelect = productRow.querySelector('.productSelect');
            let quantityInput = productRow.querySelector('.quantityInput');

            let selectedProductId = productSelect.value;
            let selectedProductName = productSelect.options[productSelect.selectedIndex].text;
            let quantity = quantityInput.value;

            if (selectedProductId && quantity) {
                productCounter++;
                lastSelectedProductId = selectedProductId; // Сохраняем последнее выбранное значение

                // Создаем новый продукт
                let newProduct = {
                    ProductId: selectedProductId,
                    ProductName: selectedProductName,
                    Quantity: parseInt(quantity)  // Преобразование в целое число
                };

                // Добавляем продукт в массив
                addedProducts.push(newProduct);

                // Отображаем продукт на странице
                let newRow = document.createElement('div');
                newRow.className = 'addedProductRow';
                newRow.innerHTML = `
                    <span>${productCounter}. </span>
                    <span>${selectedProductName}, </span>
                    <span>${quantity} шт.</span>
                    <button type="button" class="btn btn-danger removeAddedProduct">Удалить</button>
                `;
                addedProductsContainer.appendChild(newRow);

                // Очищаем поля ввода
                productSelect.value = '';
                quantityInput.value = '';

                // Восстанавливаем последнее выбранное значение
                if (lastSelectedProductId) {
                    productSelect.value = lastSelectedProductId;
                }
            }
        }
    });

    addedProductsContainer.addEventListener('click', function (event) {
        if (event.target.classList.contains('removeAddedProduct')) {
            let rowIndex = Array.from(addedProductsContainer.children).indexOf(event.target.closest('.addedProductRow'));
            addedProducts.splice(rowIndex, 1);  // Удаляем товар из массива
            event.target.closest('.addedProductRow').remove();
        }
    });

    document.getElementById('sendBatchButton').addEventListener('click', async function () {
        if (addedProducts.length > 0) {
            let details = document.getElementById('auto-expanding-textarea').value;
            let batchData = {
                Products: addedProducts
            };

            console.log("Отправляемый batchData:", batchData);  // Логируем отправляемые данные

            try {
                // Генерация ключей
                const keyPair = await generateKeys();
                const publicKey = await exportPublicKey(keyPair.publicKey);
                const privateKey = await exportPrivateKey(keyPair.privateKey);

                // Сохранение ключей в localStorage
                localStorage.setItem('publicKey', publicKey);
                localStorage.setItem('privateKey', privateKey);

                // Подпись данных
                const signature = await signData(JSON.stringify(batchData), keyPair.privateKey);



                // Отправка данных в MongoDB
                const saveResponse = await fetch('/Tracking/SaveBatch', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(batchData)
                });

                const saveResult = await saveResponse.json();

                // Подготовка данных для отправки
                const createBlockchainRequest = {
                    BatchId: saveResult.batchId, // Укажите реальный BatchId
                    UserGroupId: saveResult.userGroupId, // Укажите реальный UserGroupId
                    UserId: saveResult.userId, // Укажите реальный UserId
                    DetailsOrder: details,
                    Signature: btoa(String.fromCharCode(...new Uint8Array(signature)))
                };

                const requestWithBatch = {
                    Request: createBlockchainRequest,
                    Batch: batchData
                };

                if (saveResult.success) {
                    // Вызываем метод CreateBlockchain
                    const createBlockchainResponse = await fetch('/Tracking/CreateBlockhain', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(requestWithBatch)
                    });

                    if (createBlockchainResponse.ok) {
                        alert("Цепочка блокчейна была создана");
                        window.location.reload();
                    } else {
                        console.error('Ошибка создания блокчейна:', createBlockchainResponse.statusText);
                        alert(`Ошибка при создании блокчейна: ${createBlockchainResponse.statusText}`);
                    }

                } else {
                    alert(saveResult.message);  // Сообщение об ошибке от сервера
                }
            } catch (error) {
                console.error('Ошибка при сохранении:', error);
                window.location.reload();
            }
        } else {
            alert('Пожалуйста, добавьте хотя бы один товар.');
        }
    });

    async function generateKeys() {
        const keyPair = await window.crypto.subtle.generateKey(
            {
                name: "RSA-PSS",
                modulusLength: 2048,
                publicExponent: new Uint8Array([1, 0, 1]),
                hash: { name: "SHA-256" },
            },
            true,
            ["sign", "verify"]
        );

        return keyPair;
    }

    async function exportPublicKey(publicKey) {
        const exported = await window.crypto.subtle.exportKey("jwk", publicKey);
        return JSON.stringify(exported);
    }

    async function exportPrivateKey(privateKey) {
        const exported = await window.crypto.subtle.exportKey("jwk", privateKey);
        return JSON.stringify(exported);
    }

    async function signData(data, privateKey) {
        const encoder = new TextEncoder();
        const encodedData = encoder.encode(data);
        const signature = await window.crypto.subtle.sign(
            {
                name: "RSA-PSS",
                saltLength: 32,
            },
            privateKey,
            encodedData
        );

        return new Uint8Array(signature);
    }
});
