using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrameworkCore.RawQueryBuilder
{
    /// <summary>
    /// Empty QueryBuilder (initialized without a template), which can start both with Select() or SelectDistinct()
    /// </summary>
    public interface IEmptyQueryBuilder<TEntity> : ICompleteCommand<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Adds one column to the select clauses
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        ISelectBuilder<TEntity> Select(FormattableString column);

        /// <summary>
        /// Adds one or more columns to the select clauses
        /// </summary>
        ISelectBuilder<TEntity> Select(params FormattableString[] moreColumns);

        /// <summary>
        /// Adds one column to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        ISelectDistinctBuilder<TEntity> SelectDistinct(FormattableString select);

        /// <summary>
        /// Adds one or more columns to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        ISelectDistinctBuilder<TEntity> SelectDistinct(params FormattableString[] moreColumns);
    }
}
