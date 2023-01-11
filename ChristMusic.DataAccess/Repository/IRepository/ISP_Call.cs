using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChristMusic.DataAccess.Repository.IRepository
{
    //Repository for calling stored procedures in the database 
    public interface ISP_Call : IDisposable
    {
        //Call a stored procedure that returns a single value(example integer value)
        T Single<T>(string procedureName, DynamicParameters param = null);

        //Call a stored procedure that executes something without returning a value
        void Execute(string procedureName, DynamicParameters param = null);

        //Call a stored procedure that returns a single row 
        T OneRecord<T>(string procedureName, DynamicParameters param = null);

        //Call a stored procedure that returns a multiple rows
        IEnumerable<T> List<T>(string procedureName, DynamicParameters param = null);

        //Call a stored procedure that returns two tables
        Tuple<IEnumerable<T1>, IEnumerable<T2>> List<T1, T2>(string procedureName, DynamicParameters param = null);
    }
}
