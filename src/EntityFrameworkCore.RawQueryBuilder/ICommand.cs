using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace EntityFrameworkCore.RawQueryBuilder
{
    /// <summary>
    /// Any command (Contains Connection, SQL, and Parameters)
    /// </summary>
    public interface ICommand<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// SQL of Command
        /// </summary>
        string Sql { get; }

        /// <summary>
        /// Parameters of Command
        /// </summary>
        ParameterInfos Parameters { get; }

        /// <summary>
        /// Underlying DbSet
        /// </summary>
        DbSet<TEntity> DbSet { get; }
    }
}
