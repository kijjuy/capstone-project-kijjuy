document.querySelector("#add-to-cart-btn").addEventListener("click", async (e) => {
    let productId = e.target.dataset.productId;

    console.log(productId);

    var res = await fetch("/api/cart/" + productId, {
        method: "POST"
    });

    if(res.status == 400) {
        alert("Product already in cart.");
        return;
    }

    if(res.status == 500) {
        alert("Error adding product to cart.");
    }
    alert("Product added to cart.");
});

