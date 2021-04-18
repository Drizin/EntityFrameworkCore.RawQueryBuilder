﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EntityFrameworkCore.RawQueryBuilder
{
    /// <summary>
    /// Parses an interpolated-string SQL statement into a injection-safe statement (with parameters as @p0, @p1, etc) and a dictionary of parameter values.
    /// </summary>
    [DebuggerDisplay("{Sql} ({_parametersStr,nq})")]
    public class InterpolatedStatementParser
    {
        #region Members
        /// <summary>
        /// Injection-safe statement, with parameters as @p0, @p1, etc.
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// Dictionary of EF parameters
        /// </summary>
        public ParameterInfos Parameters { get; set; }


        private string _parametersStr;

        private static Regex _formattableArgumentRegex = new Regex(
              "{(?<ArgPos>\\d*)(:(?<Format>[^}]*))?}",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );

        private static Regex quotedVariableStart = new Regex("(^|\\s+|=|>|<|>=|<=|<>)'$",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );
        private static Regex quotedVariableEnd = new Regex("^'($|\\s+|=|>|<|>=|<=|<>)",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );

        /// <summary>
        /// By default your interpolated strings will generate parameters in the SQL statement like @p0, @p1, etc. <br />
        /// If your database does not accept @ you can change for anything else. <br />
        /// Example: if you set as ":parm" you'll get :parm1, :parm2, etc. <br />
        /// This one defines what goes into the query.
        /// </summary>
        public static Func<string, string> AutoGeneratedParameterNameFactory { get; set; } = (i) => "{" + i.ToString() + "}";

        /// <summary>
        /// By default your interpolated strings will generate parameter objects which are passed as p0, p1, etc. <br />
        /// If you need to change it (to match AutoGeneratedParameterPrefix) you can change for anything else. <br />
        /// Example: if you set as "parm" you'll get parm1, parm2, etc. <br />
        /// </summary>
        public static Func<string, string> AutoGeneratedParameterObjectNameFn { get; set; } = (i) => "p" + i.ToString(); // currently doesn't matter, we're passing positional arguments to FromSqlRaw

        #endregion

        #region Regex
        // String(maxlength) / nvarchar(maxlength) / String / nvarchar
        private static Regex regexDbTypeString = new Regex("^(String|nvarchar)\\s*(\\(\\s*(?<maxlength>\\d*)\\s*\\))?$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // StringFixedLength(length) / nchar(length) / StringFixedLength / nchar
        private static Regex regexDbTypeStringFixedLength = new Regex("^(StringFixedLength|nchar)\\s*(\\(\\s*(?<length>\\d*)\\s*\\))?$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // AnsiString(maxlength) / varchar(maxlength) / AnsiString / varchar
        private static Regex regexDbTypeAnsiString = new Regex("^(AnsiString|varchar)\\s*(\\(\\s*(?<maxlength>\\d*)\\s*\\))?$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // AnsiStringFixedLength(length) / char(length) / AnsiStringFixedLength / char
        private static Regex regexDbTypeAnsiStringFixedLength = new Regex("^(AnsiStringFixedLength|char)\\s*(\\(\\s*(?<length>\\d*)\\s*\\))?$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // text / varchar(MAX) / varchar(-1)
        private static Regex regexDbTypeText = new Regex("^(text|varchar\\s*(\\(\\s*((MAX|-1))\\s*\\)))$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        // ntext / nvarchar(MAX) / nvarchar(-1)
        private static Regex regexDbTypeNText = new Regex("^(ntext|nvarchar\\s*(\\(\\s*((MAX|-1))\\s*\\)))$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        #endregion

        #region ctor
        /// <summary>
        /// Parses an interpolated-string SQL statement into a injection-safe statement (with parameters as @p0, @p1, etc) and a dictionary of parameter values.
        /// </summary>
        /// <param name="query"></param>
        public InterpolatedStatementParser(FormattableString query) : this(query.Format, query.GetArguments())
        {
        }
        private InterpolatedStatementParser(string format, params object[] arguments)
        {
            Parameters = new ParameterInfos();

            if (string.IsNullOrEmpty(format))
                return;
            StringBuilder sb = new StringBuilder();
            var matches = _formattableArgumentRegex.Matches(format);
            int currentBlockEndPos = 0;
            int previousBlockEndPos = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                previousBlockEndPos = currentBlockEndPos;
                currentBlockEndPos = matches[i].Index + matches[i].Length;

                // unescape escaped curly braces
                string sql = format.Substring(previousBlockEndPos, matches[i].Index - previousBlockEndPos).Replace("{{", "{").Replace("}}", "}");

                // arguments[i] may not work because same argument can be used multiple times
                int argPos = int.Parse(matches[i].Groups["ArgPos"].Value);
                string argFormat = matches[i].Groups["Format"].Value;
                List<string> argFormats = argFormat.Split(new char[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).ToList();
                object arg = arguments[argPos];

                if (argFormats.Contains("raw")) // example: {nameof(Product.Name):raw}  -> won't be parametrized, we just emit raw string!
                {
                    sb.Append(sql);
                    sb.Append(arg);
                    continue;
                }
                else if (arg is FormattableString fsArg) //Support nested FormattableString
                {
                    sb.Append(sql);
                    var nestedStatement = new InterpolatedStatementParser(fsArg);
                    if (nestedStatement.Parameters.Any())
                        sb.Append(Parameters.MergeParameters(nestedStatement.Parameters, nestedStatement.Sql));
                    else
                        sb.Append(nestedStatement.Sql);
                    continue;
                }
                // If user passes " column LIKE '{variable}' ", we assume that he used single quotes incorrectly as if interpolated string was a sql literal
                if (quotedVariableStart.IsMatch(sql) && quotedVariableEnd.IsMatch(format.Substring(currentBlockEndPos)))
                {
                    sql = sql.Substring(0, sql.Length - 1); // skip starting quote
                    currentBlockEndPos++; // skip closing quote
                }

                sb.Append(sql);

                var direction = System.Data.ParameterDirection.Input;
                System.Data.DbType? dbType = null;
                if (argFormats.Contains("out"))
                    direction = System.Data.ParameterDirection.Output;

                System.Data.DbType parsedDbType;
                Match m;
                foreach (var f in argFormats)
                {
                    /*
                    if (arg is string && (m = regexDbTypeString.Match(f)) != null && m.Success) // String(maxlength) / nvarchar(maxlength) / String / nvarchar
                        arg = new DbString() 
                        { 
                            IsAnsi = false, 
                            IsFixedLength = false, 
                            Value = (string)arg, 
                            Length = (string.IsNullOrEmpty(m.Groups["maxlength"].Value) ? Math.Max(DbString.DefaultLength, ((string)arg).Length) : int.Parse(m.Groups["maxlength"].Value)) 
                        };
                    else if (arg is string && (m = regexDbTypeAnsiString.Match(f)) != null && m.Success) // AnsiString(maxlength) / varchar(maxlength) / AnsiString / varchar
                        arg = new DbString()
                        {
                            IsAnsi = true,
                            IsFixedLength = false,
                            Value = (string)arg,
                            Length = (string.IsNullOrEmpty(m.Groups["maxlength"].Value) ? Math.Max(DbString.DefaultLength, ((string)arg).Length) : int.Parse(m.Groups["maxlength"].Value))
                        };
                    else if (arg is string && (m = regexDbTypeStringFixedLength.Match(f)) != null && m.Success) // StringFixedLength(length) / nchar(length) / StringFixedLength / nchar
                        arg = new DbString()
                        {
                            IsAnsi = false,
                            IsFixedLength = true,
                            Value = (string)arg,
                            Length = (string.IsNullOrEmpty(m.Groups["length"].Value) ? ((string)arg).Length : int.Parse(m.Groups["length"].Value))
                        };
                    else if (arg is string && (m = regexDbTypeAnsiStringFixedLength.Match(f)) != null && m.Success) // AnsiStringFixedLength(length) / char(length) / AnsiStringFixedLength / char
                        arg = new DbString()
                        {
                            IsAnsi = true,
                            IsFixedLength = true,
                            Value = (string)arg,
                            Length = (string.IsNullOrEmpty(m.Groups["length"].Value) ? ((string)arg).Length : int.Parse(m.Groups["length"].Value))
                        };
                    else if (arg is string && (m = regexDbTypeText.Match(f)) != null && m.Success) // text / varchar(MAX) / varchar(-1)
                        arg = new DbString()
                        {
                            IsAnsi = false,
                            IsFixedLength = true,
                            Value = (string)arg,
                            Length = int.MaxValue
                        };
                    else if (arg is string && (m = regexDbTypeNText.Match(f)) != null && m.Success) // ntext / nvarchar(MAX) / nvarchar(-1)
                        arg = new DbString()
                        {
                            IsAnsi = true,
                            IsFixedLength = true,
                            Value = (string)arg,
                            Length = int.MaxValue
                        };

                    else if (!(arg is DbString) && dbType == null && Enum.TryParse<System.Data.DbType>(value: f, ignoreCase: true, result: out parsedDbType))
                    {
                        dbType = parsedDbType;
                    }
                    */

                    if (dbType == null && Enum.TryParse<System.Data.DbType>(value: f, ignoreCase: true, result: out parsedDbType))
                    {
                        dbType = parsedDbType;
                    }

                    //TODO: parse SqlDbTypes?
                    // https://stackoverflow.com/questions/35745226/net-system-type-to-sqldbtype
                    // https://gist.github.com/tecmaverick/858392/53ddaaa6418b943fa3a230eac49a9efe05c2d0ba
                }
                sb.Append( Parameters.Add( arg, dbType, direction ) );
            }
            string lastPart = format.Substring(currentBlockEndPos).Replace("{{", "{").Replace("}}", "}");
            sb.Append(lastPart);
            Sql = sb.ToString();
            _parametersStr = string.Join(", ", Parameters.ParameterNames.ToList().Select(n => "@" + n + "='" + Convert.ToString(Parameters.Get<dynamic>(n)) + "'"));
        }
        #endregion

        /// <summary>
        /// Merges parameters from this query/statement into a CommandBuilder. <br />
        /// Checks for name clashes, and will rename parameters (in CommandBuilder) if necessary. <br />
        /// If some parameter is renamed the underlying Sql statement will have the new parameter names replaced by their new names.<br />
        /// This method does NOT append Parser SQL to CommandBuilder SQL (you may want to save this SQL statement elsewhere)
        /// </summary>
        public void MergeParameters(ParameterInfos target)
        {
            string newSql = target.MergeParameters(Parameters, Sql);
            if (newSql != null)
            {
                Sql = newSql;
                _parametersStr = string.Join(", ", Parameters.ParameterNames.ToList().Select(n => "@" + n + "='" + Convert.ToString(Parameters.Get<dynamic>(n)) + "'"));
                // filter parameters in Sql were renamed and won't match the previous passed filters - discard original parameters to avoid reusing wrong values
                Parameters = null;
            }
        }

    }
}
