namespace Regata.Utilities.Deploy.BackUp
{
  public class Program
  {
    private static void Main()
    {
      var bm = new BackUpSystem();
      bm.Notify(bm.BackUpDataBase(), bm.MoveFileToGDriveFolder());
    } // Main
  } // class Program
} //namespace BackUpDB
