namespace Regata.Utilities.Deploy.BackUp
{
    // TODO: add gdrive api https://developers.google.com/drive/api/v3/quickstart/dotnet
    public class Program
    {
        private static void Main()
        {
            var bm = new BackUpSystem();
            bm.Notify(bm.BackUpDataBase(), bm.MoveFileToGDriveFolder());
        }
    } // class Program
} // namespace BackUpDB
