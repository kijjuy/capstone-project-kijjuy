document.addEventListener("DOMContentLoaded", () => {

    // Attach delete event to all delete buttons
    document.querySelectorAll(".cart-delete").forEach(btn => {
        btn.addEventListener("click", async (event) => {
            const productId = btn.dataset.productId;
            if (!productId) return;

            const res = await fetch("/api/cart/" + productId, {
                method: "DELETE"
            });

            if (res.status === 400) {
                alert("Error removing item from cart.");
                return;
            }

            // Optionally remove the item from DOM instead of reload
            // btn.closest(".row").remove();

            window.location.reload();
        });
    });    
});
    
