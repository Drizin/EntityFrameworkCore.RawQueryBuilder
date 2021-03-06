using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrameworkCore.RawQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more having clauses, which can still add more clauses to having
    /// </summary>
    public interface IGroupByHavingBuilder<TEntity> : ICompleteCommand<TEntity>
        where TEntity : class
    {

        /// <summary>
        /// Adds a new condition to having clauses.
        /// </summary>
        /// <param name="having"></param>
        /// <returns></returns>
        IGroupByHavingBuilder<TEntity> Having(FormattableString having);

        /// <summary>
        /// Adds a new condition to orderby clauses.
        /// </summary>
        IOrderByBuilder<TEntity> OrderBy(FormattableString orderBy);
    }
}
