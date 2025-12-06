document.querySelectorAll(".delete-btn").forEach(btn => {
    btn.addEventListener("click", async () => {
	let productId = btn.dataset.productId;
	console.log("Delete item hit with value " + productId);
    	let shouldDelete = confirm(`Are you sure you want to delete product with id=${productId}?`);
    	if(!shouldDelete) {
    	    return;
    	}

    	let res = await fetch("/api/products/" + productId, {
    	    method: "DELETE"
    	});
    	if(res.status != 204) {
    	    alert("Error deleting product. Please check application logs.");
    	    return;
    	}
    	window.location.reload();
    });
});

