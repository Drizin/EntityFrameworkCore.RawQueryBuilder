using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrameworkCore.RawQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more clause in where, which can still add more clauses to where
    /// </summary>
    public interface IWhereBuilder<TEntity> : ICompleteCommand<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        IWhereBuilder<TEntity> Where(Filter filter);

        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        IWhereBuilder<TEntity> Where(Filters filter);

        /// <summary>
        /// Adds a new condition to where clauses. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into EF parameters.
        /// </summary>
        IWhereBuilder<TEntity> Where(FormattableString filter);

        /// <summary>
        /// Adds a new condition to groupby clauses.
        /// </summary>
        IGroupByBuilder<TEntity> GroupBy(FormattableString groupBy);

        /// <summary>
        /// Adds a new condition to orderby clauses.
        /// </summary>
        IOrderByBuilder<TEntity> OrderBy(FormattableString orderBy);
    }
}
