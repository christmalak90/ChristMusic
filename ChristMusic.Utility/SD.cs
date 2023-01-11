using System;
using System.Collections.Generic;
using System.Text;

namespace ChristMusic.Utility
{
    //This class contains some static or constante objects used in the project
    public static class SD
    {
        #region CoverType Store Procedure Names in the Database

        public const string Proc_CoverType_Create = "usp_CreateCoverType";
        public const string Proc_CoverType_Get = "usp_GetCoverType";
        public const string Proc_CoverType_GetAll = "usp_GetCoverTypes";
        public const string Proc_CoverType_Update = "usp_UpdateCoverType";
        public const string Proc_CoverType_Delete = "usp_DeleteCoverType";

        #endregion

        #region User Roles in the Application

        public const string Role_IndividualCustomer = "Individual Customer";
        public const string Role_CompanyCustomer = "Company Customer";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";

        #endregion

        #region Session Names

        public static string NbrOfProductInShoppingCartSession = "Shopping Cart Session"; //Stores the number of items that the user has in the shopping cart

        #endregion

        //Method to return price of single product based on quantity in the shopping Cart
        public static double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if(quantity < 50)
            {
                return price;
            }
            else
            {
                if(quantity < 100)
                {
                    return price50;
                }
                else
                {
                    return price100;
                }
            }
        }

        //This method converts string to html
        public static string ConvertToRawHtml(string source)
        {
            if (source != null)
            {
                char[] array = new char[source.Length];
                int arrayIndex = 0;
                bool inside = false;

                for (int i = 0; i < source.Length; i++)
                {
                    char let = source[i];
                    if (let == '<')
                    {
                        inside = true;
                        continue;
                    }
                    if (let == '>')
                    {
                        inside = false;
                        continue;
                    }
                    if (!inside)
                    {
                        array[arrayIndex] = let;
                        arrayIndex++;
                    }
                }
                return new string(array, 0, arrayIndex);
            }
            return null;
        }

        #region Order Status in the Application

        public const string OrderStatusPending = "Pending";
        public const string OrderStatusApproved = "Approved";
        public const string OrderStatusInProcess = "Processing";
        public const string OrderStatusShipped = "Shipped";
        public const string OrderStatusCancelled = "Cancelled";

        #endregion

        #region Payment Status in the Application

        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment";
        public const string PaymentStatusRejected = "Rejected";
        public const string PaymentStatusRefunded = "Refunded";
        public const string PaymentStatusCancelled = "Cancelled";
        #endregion
    }
}
