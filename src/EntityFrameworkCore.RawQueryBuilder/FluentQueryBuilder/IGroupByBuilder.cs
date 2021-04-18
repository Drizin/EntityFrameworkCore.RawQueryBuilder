using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrameworkCore.RawQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more groupby clauses, which can still add more clauses to groupby
    /// </summary>
    public interface IGroupByBuilder<TEntity> : ICompleteCommand<TEntity>
        where TEntity : class
    {
        IGroupByBuilder<TEntity> GroupBy(FormattableString groupBy);
        IGroupByHavingBuilder<TEntity> Having(FormattableString having);
        IOrderByBuilder<TEntity> OrderBy(FormattableString orderBy);
    }
}
