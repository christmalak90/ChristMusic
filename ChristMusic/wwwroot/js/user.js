var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/User/GetAll"
        },
        "columns": [
            { "data": "name", "width": "14%" },
            { "data": "email", "width": "14%" },
            { "data": "phoneNumber", "width": "14%" },
            { "data": "company.name", "width": "14%" },
            { "data": "role", "width": "" },
            {
                "data": "emailConfirmed",
                "render": function (data) {
                    if (data)
                    {
                        return `<input type="checkbox" disabled checked/>`
                    }
                    else
                    {
                        return `<input type="checkbox" disabled/>`
                    }
                },
                "width": "14%"
            },
            {
                "data": {
                    id: "id", lockoutEnd: "lockoutEnd" //lockoutEnd is a field in the AspNetUsers table in the database that specifies the time in which a user is locked
                },
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();
                    if (lockout > today) {
                        //the user is in locked state

                        //Code to Unlock the user
                        return `
                            <div class="text-center">
                               <a onclick=LockUnLock("${data.id}") class="btn btn-danger text-white" style="cursor:pointer; width:100px">
                                    <i class="fas fa-lock-open"></i> Unlock
                                </a>
                            </div>
                            `;
                    }
                    else{
                        //the user is in Unlocked state

                        //Code to Lock the user
                        return `
                            <div class="text-center">
                               <a onclick=LockUnLock("${data.id}") class="btn btn-success text-white" style="cursor:pointer; width:100px">
                                    <i class="fas fa-lock"></i> Lock
                                </a>
                            </div>
                            `;
                    }
                }, "width": "16%"
            }
        ]
    });
}

function LockUnLock(id) {
    $.ajax({
        type: "POST",
        url: "/Admin/User/LockUnLock",
        data: JSON.stringify(id),
        contentType:"application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
            else {
                toastr.error(data.message);
            }
        }
    });
}