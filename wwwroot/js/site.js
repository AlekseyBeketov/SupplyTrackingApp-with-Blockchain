document.addEventListener('DOMContentLoaded', function () {
    let productContainer = document.getElementById('productContainer');
    let addedProductsContainer = document.getElementById('addedProductsContainer');
    let productRow = productContainer.querySelector('.productRow');
    let productCounter = 0;
    let lastSelectedProductId = null;
    let addedProducts = [];

    const accordionState = localStorage.getItem('accordionOpen');
    const collapseElement = document.getElementById('collapseOne');
    if (accordionState === 'true') {
        collapseElement.classList.add('show');
    }

    collapseElement.addEventListener('show.bs.collapse', () => {
        localStorage.setItem('accordionOpen', 'true');
    });

    collapseElement.addEventListener('hide.bs.collapse', () => {
        localStorage.setItem('accordionOpen', 'false');
    });

    document.addEventListener('DOMContentLoaded', function () {
        const transportAccordionState = localStorage.getItem('transportAccordionOpen');
        const transportCollapseElements = document.querySelectorAll('[id^="collapseTransporter_"]');

        transportCollapseElements.forEach(collapseElement => {
            if (transportAccordionState === 'true') {
                collapseElement.classList.add('show');
            }

            collapseElement.addEventListener('show.bs.collapse', () => {
                localStorage.setItem('transportAccordionOpen', 'true');
            });

            collapseElement.addEventListener('hide.bs.collapse', () => {
                localStorage.setItem('transportAccordionOpen', 'false');
            });
        });
    });

    document.getElementById('vendorSelect').addEventListener('change', function () {
        localStorage.setItem('accordionOpen', 'true');
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
                lastSelectedProductId = selectedProductId;

                let newProduct = {
                    ProductId: selectedProductId,
                    ProductName: selectedProductName,
                    Quantity: parseInt(quantity)
                };

                addedProducts.push(newProduct);

                let newRow = document.createElement('div');
                newRow.className = 'addedProductRow';
                newRow.innerHTML = `
                    <span>${productCounter}. </span>
                    <span><b>${selectedProductName}</b>, </span>
                    <span>${quantity} шт.</span>
                    <button type="button" style="border: 1px solid #002F55" class="btn-outline-primary removeAddedProduct">Удалить</button>
                `;
                addedProductsContainer.appendChild(newRow);

                productSelect.value = '';
                quantityInput.value = '';

                if (lastSelectedProductId) {
                    productSelect.value = lastSelectedProductId;
                }
            }
        }
    });

    addedProductsContainer.addEventListener('click', function (event) {
        if (event.target.classList.contains('removeAddedProduct')) {
            let rowIndex = Array.from(addedProductsContainer.children).indexOf(event.target.closest('.addedProductRow'));
            addedProducts.splice(rowIndex, 1);
            event.target.closest('.addedProductRow').remove();
        }
    });


    document.getElementById('sendBatchButton').addEventListener('click', async function () {
        if (addedProducts.length > 0) {
            let details = document.getElementById('auto-expanding-textarea').value;
            let batchData = {
                Products: addedProducts
            };

            try {
                const saveResponse = await fetch('/Tracking/SaveBatch', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(batchData)
                });

                const saveResult = await saveResponse.json();

                const createBlockchainRequest = {
                    BatchId: saveResult.batchId,
                    UserGroupId: saveResult.userGroupId,
                    UserId: saveResult.userId,
                    DetailsOrder: details,
                };
                console.log('createBlockchainRequest:', createBlockchainRequest);

                const requestWithBatch = {
                    Request: createBlockchainRequest,
                    Batch: batchData
                };

                if (saveResult.success) {
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
                        alert(`Ошибка при создании блокчейна: ${createBlockchainResponse.statusText}`);
                    }
                } else {
                    alert(saveResult.message);
                }
            } catch (error) {
                window.location.reload();
            }
        } else {
            alert('Пожалуйста, добавьте хотя бы один товар.');
        }
    });
});
