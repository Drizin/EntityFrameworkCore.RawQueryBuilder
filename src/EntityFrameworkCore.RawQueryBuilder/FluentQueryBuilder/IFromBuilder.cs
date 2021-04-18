using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrameworkCore.RawQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more from clauses, which can still add more clauses to from
    /// </summary>
    public interface IFromBuilder<TEntity> : ICompleteCommand<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Adds a new table to from clauses. <br />
        /// "FROM" word is optional. <br />
        /// You can add an alias after table name. <br />
        /// You can also add INNER JOIN, LEFT JOIN, etc (with the matching conditions).
        /// </summary>
        IFromBuilder<TEntity> From(FormattableString from);

        /// <summary>
        /// Adds a new group of conditions to where clauses.
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
        /// Adds a new column to orderby clauses.
        /// </summary>
        IOrderByBuilder<TEntity> OrderBy(FormattableString orderBy);
    }
}
