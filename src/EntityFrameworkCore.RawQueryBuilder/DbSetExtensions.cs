using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace EntityFrameworkCore.RawQueryBuilder
{
    /// <summary>
    /// Extends IDbConnection to easily build QueryBuilder or FluentQueryBuilder
    /// </summary>
    public static class DbSetExtensions
    {
        /// <summary>
        /// Creates a new empty FluentQueryBuilder over current connection
        /// </summary>
        public static IEmptyQueryBuilder<TEntity> FluentQueryBuilder<TEntity>(this DbSet<TEntity> dbSet)
            where TEntity : class
        {
            return new FluentQueryBuilder<TEntity>(dbSet);
        }


        /// <summary>
        /// Creates a new QueryBuilder over current connection
        /// </summary>
        /// <param name="dbSet"></param>
        /// <param name="query">You can use "{where}" or "/**where**/" in your query, and it will be replaced by "WHERE + filters" (if any filter is defined). <br />
        /// You can use "{filters}" or "/**filters**/" in your query, and it will be replaced by "filters" (without where) (if any filter is defined).
        /// </param>
        public static QueryBuilder<TEntity> RawQueryBuilder<TEntity>(this DbSet<TEntity> dbSet, FormattableString query)
            where TEntity : class
        {
            return new QueryBuilder<TEntity>(dbSet, query);
        }

        /// <summary>
        /// Creates a new empty QueryBuilder over current connection
        /// </summary>
        /// <param name="dbSet"></param>
        public static QueryBuilder<TEntity> RawQueryBuilder<TEntity>(this DbSet<TEntity> dbSet)
            where TEntity : class
        {
            return new QueryBuilder<TEntity>(dbSet);
        }

        /// <summary>
        /// Creates a new CommandBuilder over current connection
        /// </summary>
        /// <param name="dbSet"></param>
        /// <param name="command">SQL command</param>
        public static CommandBuilder<TEntity> CommandBuilder<TEntity>(this DbSet<TEntity> dbSet, FormattableString command)
            where TEntity : class
        {
            return new CommandBuilder<TEntity>(dbSet, command);
        }

        /// <summary>
        /// Creates a new empty CommandBuilder over current connection
        /// </summary>
        public static CommandBuilder<TEntity> CommandBuilder<TEntity>(this DbSet<TEntity> dbSet)
            where TEntity : class
        {
            return new CommandBuilder<TEntity>(dbSet);
        }

    }
}
