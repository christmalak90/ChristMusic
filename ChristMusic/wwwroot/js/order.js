var dataTable;

$(document).ready(function () {
    var url = window.location.search; //find the url of the web page
    if (url.includes("orderInprocess"))
    {
        loadDataTable("orderInprocess");
    }
    else
    {
        if (url.includes("paymentPending"))
        {
            loadDataTable("paymentPending");
        }
        else
        {
            if (url.includes("orderCompleted"))
            {
                loadDataTable("orderCompleted");
            }
            else
            {
                if (url.includes("orderRejected"))
                {
                    loadDataTable("orderRejected");
                }
                else
                {
                    loadDataTable("allOrders");
                }
            }
        }
    }
});

function loadDataTable(orderStatus) {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Order/GetOrderList?orderStatus=" + orderStatus
        },
        "columns": [
            { "data": "id", "width": "5%" },
            { "data": "name", "width": "10%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "applicationUser.email", "width": "15%" },
            { "data": "orderStatus", "width": "15%" },
            { "data": "paymentStatus", "width": "10%" },
            { "data": "orderTotal", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                                <a href="/Admin/Order/OrderDetails/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                    <i class="fas fa-edit"></i>
                                </a>
                            </div>
                            `;
                }, "width": "5%"
            }
        ]
    });
}
