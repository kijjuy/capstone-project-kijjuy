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
    public static T MapDataToModel<T>(SqliteDataReader reader)
    {
	var type = typeof(T);
	var props = type.GetProperties();

	foreach (var prop in props) 
	{
	    var colName = prop.GetCustomAttribute<ColumnAttribute>();
	    Console.WriteLine($"colName: {colName.Name}");
	    //TODO: Have names, now get them from reader
	}

	throw new NotImplementedException();
    }
}
