using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.Data.Sqlite;

namespace app.Repositories;

/***
 * <summary>
 * Helper class that maps data from dictionary of
 * sql row column:value to model using reflection.
 * </summary>
 */
public class ReaderMapper
{

    private readonly ILogger<ReaderMapper> _logger;

    public ReaderMapper(ILogger<ReaderMapper> logger)
    {
        _logger = logger;
    }

    /**
     * <summary>
     * Creates a new object of type T and iterates through rowDict.Keys.
     * For each key, attempts for find property of T that has matching ColumnAttribute value.
     * If found, load value from rowDict[key] into new T object. If not found,
     * throw ArgumentException.
     * </summary>
     */
    public T MapDataToModel<T>(Dictionary<String, object> rowDict) where T : new()
    {
        var type = typeof(T);
        var props = type.GetProperties();
        var obj = new T();

        if (props.Length == 0)
        {
            throw new ArgumentException("Cannot parse to a model with no properties.");
        }

        //get only props that have col attr
        var propsWithCols = new List<PropertyInfo>();
        foreach (var prop in props)
        {
            try
            {
                var colAttr = prop.GetCustomAttribute<ColumnAttribute>();
                if (colAttr == null)
                {
                    throw new NullReferenceException("Prop does not have col attr");
                }
                propsWithCols.Add(prop);
            }
            catch (NullReferenceException nle)
            {
                _logger.LogDebug($"tried to get col prop for prop that doesn't have col value. Err={nle.Message}");
                continue;
            }
        }

        _logger.LogDebug($"Count of propsWithCols = {propsWithCols.Count}");

        if (propsWithCols.Count() == 0)
        {
            _logger.LogWarning("Attempted to parse to model with no column attribues.");
        }

        if (rowDict.Keys.Count() == 0)
        {
            _logger.LogWarning("Attempted to parse empty sql data");
            return obj;
        }

        foreach (String key in rowDict.Keys)
        {
            _logger.LogDebug($"checking for key={key}");
            var matchingProp = propsWithCols.Where(p => p.GetCustomAttribute<ColumnAttribute>().Name
                .Equals(key)).FirstOrDefault();

            // Sql data doesn't match prop data, throw exception
            if (matchingProp == null)
            {
                _logger.LogWarning("Tried to match sql column name to prop column value that doesn't exist.");
                throw new ArgumentException($"No matching property for sql column with name={key}");
            }

            _logger.LogDebug($"Got prop with name={matchingProp.Name}");

            if (matchingProp.PropertyType.Equals(typeof(DateTime)))
            {
                _logger.LogDebug($"parsing date value...");
                try
                {
                    DateTime dateTimeVal = DateTime.Parse((String)rowDict[key]);
                    matchingProp.SetValue(obj, dateTimeVal);
                }
                catch (FormatException fe)
                {
                    _logger.LogWarning($"Empty/Invalid date in products. Err={fe.Message}");
                    continue;
                }
                continue;
            }

            matchingProp.SetValue(obj, rowDict[key]);
        }

        return obj;
    }

    /**
     * <summary>
     * Creates a dictionary with each key being the column name and value being the column value from the database.
     * This can be used to map any type T with ColumnAttributes.
     * </summary>
     */
    public static Dictionary<String, object> CreateSqlDictionary(SqliteDataReader reader)
    {
        Dictionary<String, object> rowDict = new Dictionary<string, object>();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            String name = reader.GetName(i);
            var val = reader.GetValue(i);
            rowDict[name] = val;
        }
        return rowDict;
    }
}
