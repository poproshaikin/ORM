namespace ORM.Core.Exceptions;

internal class MissingTableException : Exception
{
   private string _tableName;

   public MissingTableException(string tableName) : base($"No such table in database with name \"{tableName}\"")
   {
      _tableName = tableName;
   }
   public MissingTableException(string message, string tableName) : base(message)
   {
      _tableName = tableName;
   }
}