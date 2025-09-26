using Microsoft.Data.Sqlite;
using app.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace app.Repositories;

/***
 * Helper class that maps data from SqlDataReader to models
 * using reflection.
 */
public static class ReaderMapper
{
    public static T MapDataToModel<T>(SqliteDataReader reader) where T: new()
    {
	var type = typeof(T);
	var props = type.GetProperties();
	var obj = new T();

	foreach (var prop in props) 
	{
	    var colAttr = prop.GetCustomAttribute<ColumnAttribute>();

	    int loc = reader.GetOrdinal(colAttr.Name);
	    var val = reader.GetValue(loc);

	    Console.WriteLine($"setting value of {colAttr.Name} to {val}.");
	    Console.WriteLine($"type of current prop: {prop.PropertyType}");

	    if(prop.PropertyType.Equals(typeof(DateTime))) 
	    {
		Console.WriteLine($"parsing DateTime value...");
		DateTime dateTimeVal = DateTime.Parse((String)val);
		prop.SetValue(obj, dateTimeVal);
		continue;
	    }

	    prop.SetValue(obj, val);

	}

	return obj;
    }
}
