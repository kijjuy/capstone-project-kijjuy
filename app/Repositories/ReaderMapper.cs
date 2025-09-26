using Microsoft.Data.Sqlite;
using app.Models;
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

        foreach (var prop in props)
        {
            var colAttr = prop.GetCustomAttribute<ColumnAttribute>();

            var val = rowDict[colAttr.Name];

            _logger.LogDebug($"setting value of {colAttr.Name} to {val}.");
            _logger.LogDebug($"type of current prop: {prop.PropertyType}");

            //Have to parse date string to get DateTime Value
            if (prop.PropertyType.Equals(typeof(DateTime)))
            {
                _logger.LogDebug($"parsing date value...");
                try
                {
                    DateTime dateTimeVal = DateTime.Parse((String)val);
                    prop.SetValue(obj, dateTimeVal);
                }
                catch (FormatException fe)
                {
                    _logger.LogWarning($"Empty/Invalid date in products. Err={fe.Message}");
                    continue;
                }
                continue;
            }

            prop.SetValue(obj, val);

        }

        return obj;
    }
}
