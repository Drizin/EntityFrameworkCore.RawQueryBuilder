using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EntityFrameworkCore.RawQueryBuilder
{
    /// <summary>
    /// FluentQueryBuilder allows to build queries using a Fluent-API interface
    /// </summary>
    public class FluentQueryBuilder<TEntity> : IEmptyQueryBuilder<TEntity>, ISelectBuilder<TEntity>, ISelectDistinctBuilder<TEntity>, IFromBuilder<TEntity>, IWhereBuilder<TEntity>, IGroupByBuilder<TEntity>, IGroupByHavingBuilder<TEntity>, IOrderByBuilder<TEntity>, ICompleteCommand<TEntity>
        where TEntity : class
    {

        #region Members
        private readonly QueryBuilder<TEntity> _queryBuilder;
        private readonly List<string> _selectColumns = new List<string>();
        private readonly List<string> _fromTables = new List<string>();
        private readonly List<string> _orderBy = new List<string>();
        private readonly List<string> _groupBy = new List<string>();
        private readonly List<string> _having = new List<string>();
        private int? _rowCount = null;
        private int? _offset = null;
        private bool _isSelectDistinct = false;
        #endregion

        #region ctors
        /// <summary>
        /// New empty FluentQueryBuilder. <br />
        /// Should be constructed using .Select(), .From(), .Where(), etc.
        /// </summary>
        /// <param name="dbSet"></param>
        public FluentQueryBuilder(DbSet<TEntity> dbSet)
        {
            _queryBuilder = new QueryBuilder<TEntity>(dbSet);
        }
        #endregion

        #region Fluent API methods
        /// <summary>
        /// Adds one column to the select clauses
        /// </summary>
        public ISelectBuilder<TEntity> Select(FormattableString column)
        {
            var parsedStatement = new InterpolatedStatementParser(column);
            parsedStatement.MergeParameters(this.Parameters);
            _selectColumns.Add(parsedStatement.Sql);
            return this;
        }

        /// <summary>
        /// Adds one or more columns to the select clauses
        /// </summary>
        public ISelectBuilder<TEntity> Select(params FormattableString[] moreColumns)
        {
            //Select(column);
            foreach (var col in moreColumns)
                Select(col);
            return this;
        }

        /// <summary>
        /// Adds one column to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        public ISelectDistinctBuilder<TEntity> SelectDistinct(FormattableString select)
        {
            _isSelectDistinct = true;
            var parsedStatement = new InterpolatedStatementParser(select);
            parsedStatement.MergeParameters(this.Parameters);
            _selectColumns.Add(parsedStatement.Sql);
            return this;
        }

        /// <summary>
        /// Adds one or more columns to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        public ISelectDistinctBuilder<TEntity> SelectDistinct(params FormattableString[] moreColumns)
        {
            //SelectDistinct(select);
            foreach (var col in moreColumns)
                SelectDistinct(col);
            return this;
        }

        /// <summary>
        /// Adds a new table to from clauses. <br />
        /// "FROM" word is optional. <br />
        /// You can add an alias after table name. <br />
        /// You can also add INNER JOIN, LEFT JOIN, etc (with the matching conditions).
        /// </summary>
        public IFromBuilder<TEntity> From(FormattableString from)
        {
            var parsedStatement = new InterpolatedStatementParser(from);
            parsedStatement.MergeParameters(this.Parameters);
            string sql = parsedStatement.Sql;
            if (!_fromTables.Any() && !Regex.IsMatch(sql, "\\b FROM \\b", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace))
                sql = "FROM " + sql;
            _fromTables.Add(sql);
            return this;
        }
        //TODO: create options with InnerJoin, LeftJoin, RightJoin, FullJoin, CrossJoin? Create overloads with table alias?


        /// <summary>
        /// Adds a new column to orderby clauses.
        /// </summary>
        public IOrderByBuilder<TEntity> OrderBy(FormattableString orderBy)
        {
            var parsedStatement = new InterpolatedStatementParser(orderBy);
            parsedStatement.MergeParameters(this.Parameters);
            _orderBy.Add(parsedStatement.Sql);
            return this;
        }

        /// <summary>
        /// Adds a new column to groupby clauses.
        /// </summary>
        public IGroupByBuilder<TEntity> GroupBy(FormattableString groupBy)
        {
            var parsedStatement = new InterpolatedStatementParser(groupBy);
            parsedStatement.MergeParameters(this.Parameters);
            _groupBy.Add(parsedStatement.Sql);
            return this;
        }

        /// <summary>
        /// Adds a new condition to having clauses.
        /// </summary>
        public IGroupByHavingBuilder<TEntity> Having(FormattableString having)
        {
            var parsedStatement = new InterpolatedStatementParser(having);
            parsedStatement.MergeParameters(this.Parameters);
            _having.Add(parsedStatement.Sql);
            return this;
        }

        /// <summary>
        /// Adds offset and rowcount clauses
        /// </summary>
        public ICompleteCommand<TEntity> Limit(int offset, int rowCount)
        {
            _offset = offset;
            _rowCount = rowCount;
            return this;
        }

        #endregion

        #region Where overrides
        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        public IWhereBuilder<TEntity> Where(Filter filter)
        {
            _queryBuilder.Where(filter);
            return this;
        }

        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        public IWhereBuilder<TEntity> Where(Filters filters)
        {
            _queryBuilder.Where(filters);
            return this;
        }


        /// <summary>
        /// Adds a new condition to where clauses. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into EF parameters.
        /// </summary>
        public IWhereBuilder<TEntity> Where(FormattableString filter)
        {
            _queryBuilder.Where(filter);
            return this;
        }
        #endregion


        #region ICompleteCommand

        #region Sql
        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public string Sql
        {
            get
            {
                StringBuilder finalSql = new StringBuilder();

                // If Query Template is provided, we assume it contains both SELECT and FROMs
                if (_selectColumns.Any())
                    finalSql.AppendLine($"SELECT {(_isSelectDistinct ? "DISTINCT " : "")}{string.Join(", ", _selectColumns)}");
                else
                    finalSql.AppendLine($"SELECT {(_isSelectDistinct ? "DISTINCT " : "")}*");

                if (_fromTables.Any())
                    finalSql.AppendLine($"{string.Join(Environment.NewLine, _fromTables)}"); //TODO: inner join and left/outer join shortcuts?

                string filters = _queryBuilder.GetFilters();
                if (filters != null)
                    finalSql.AppendLine("WHERE " + filters);

                if (_groupBy.Any())
                    finalSql.AppendLine($"GROUP BY {string.Join(", ", _groupBy)}");
                if (_having.Any())
                    finalSql.AppendLine($"HAVING {string.Join(" AND ", _having)}");
                if (_orderBy.Any())
                    finalSql.AppendLine($"ORDER BY {string.Join(", ", _orderBy)}");
                if (_rowCount != null)
                    finalSql.AppendLine($"OFFSET {_offset ?? 0} ROWS FETCH NEXT {_rowCount} ROWS ONLY"); // TODO: PostgreSQL? "LIMIT row_count OFFSET offset"

                return finalSql.ToString();
            }
        }
        #endregion

        /// <summary>
        /// Parameters of Query
        /// </summary>
        public ParameterInfos Parameters => _queryBuilder.Parameters;

        /// <summary>
        /// Underlying connection
        /// </summary>
        public DbSet<TEntity> DbSet => _queryBuilder.DbSet;
        #endregion

    }
}
