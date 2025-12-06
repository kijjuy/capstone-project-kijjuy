document.querySelector("#filter-reveal").addEventListener("click", toggleFilter);

console.log("hello");

function toggleFilter() {
    let filterElement = document.querySelector("#filter-root");
    filterElement.hidden = !filterElement.hidden
}
