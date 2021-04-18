using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace EntityFrameworkCore.RawQueryBuilder
{
    /// <summary>
    /// Any command (Contains Connection, SQL, and Parameters) which is complete for execution.
    /// </summary>
    public interface ICompleteCommand<TEntity> : ICommand<TEntity>
        where TEntity : class
    {
    }
}
