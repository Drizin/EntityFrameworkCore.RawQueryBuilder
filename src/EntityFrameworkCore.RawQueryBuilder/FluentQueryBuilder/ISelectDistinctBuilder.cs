using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrameworkCore.RawQueryBuilder
{
    /// <summary>
    /// QueryBuilder which is preparing a SELECT DISTINCT statement
    /// </summary>
    public interface ISelectDistinctBuilder<TEntity> : ICompleteCommand<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Adds one column to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        ISelectDistinctBuilder<TEntity> SelectDistinct(FormattableString select);

        /// <summary>
        /// Adds one or more columns to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        ISelectDistinctBuilder<TEntity> SelectDistinct(params FormattableString[] moreColumns);


        /// <summary>
        /// Adds a new table to from clauses. <br />
        /// "FROM" word is optional. <br />
        /// You can add an alias after table name. <br />
        /// You can also add INNER JOIN, LEFT JOIN, etc (with the matching conditions).
        /// </summary>
        IFromBuilder<TEntity> From(FormattableString from);
    }
}
