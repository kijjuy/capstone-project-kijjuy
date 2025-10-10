using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace app.Repositories;

/***
 * Helper class that maps data from dictionary of
 * sql row column:value to model using reflection.
 */
public class ReaderMapper
{

    private readonly ILogger<ReaderMapper> _logger;

    public ReaderMapper(ILogger<ReaderMapper> logger)
    {
        _logger = logger;
    }

    /**
     *
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
}
