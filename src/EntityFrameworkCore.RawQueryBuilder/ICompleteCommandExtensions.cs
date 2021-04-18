using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.RawQueryBuilder
{
    /// <summary>
    /// ICompleteCommands are "ready to run" - the only extension is ToQueryable{TEntity} which allows us to rely on EF extensions like ToList, Where, etc.
    /// </summary>
    public static class ICompleteCommandExtensions
    {

        /// <summary>
        /// Builds the final SQL and Parameters, passes to Entity Framework, and get the result IQueryable{TEntity}
        /// which can be used as you like (ToList, Where, etc).
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> AsQueryable<TEntity>(this ICompleteCommand<TEntity> command)
            where TEntity : class
        {
            return command.DbSet.FromSqlRaw(sql: command.Sql, parameters: command.Parameters.Items);
        }

        ///// <summary>
        ///// Build the current SQL command
        ///// </summary>
        //public static string CurrentSql<TEntity>(this ICompleteCommand<TEntity> command)
        //    where TEntity : class
        //{
        //    return command.Sql;
        //}

        ///// <summary>
        ///// Build the current SQL command
        ///// </summary>
        //public static object[] CurrentParameters<TEntity>(this ICompleteCommand<TEntity> command)
        //    where TEntity : class
        //{
        //    return command.Parameters.Items;
        //}
    }
}
