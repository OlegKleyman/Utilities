namespace Omego.Utilities.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Demo;

    using FluentAssertions;

    using Xbehave;

    using Xunit;

    public class ImportRecordsFeature
    {
        public static IEnumerable<object> ImportData = new[]
                                                           {
                                                               new[]
                                                                   {
                                                                       Path.GetFullPath("Resources"),
                                                                       Path.Combine(
                                                                           Path.GetTempPath(),
                                                                           Path.GetTempFileName(),
                                                                           ".mdf")
                                                                   }
                                                           };

        [Scenario(Skip = "Need to implement console")]
        [MemberData("ImportData")]
        public void DemoToImportDataIntoNewSqlServerFlatFile(
            string dataDirectoryPath,
            string newMdfFilePath)
        {
            $"Given I have a set of data I want to import in {dataDirectoryPath}"._(() => { });
            $"And it needs to be imported into the {newMdfFilePath}"._(() => { });
            "When I import the data"._(() => Program.Main(dataDirectoryPath, newMdfFilePath));
            "Then I the data should be imported"._(
                () =>
                    {
                        var connectionString =
                            FormattableString.Invariant(
                                $@"Data Source=(LocalDB)\v11.0;AttachDbFilename={newMdfFilePath};Integrated Security=True");

                        using (var connection = new SqlConnection(connectionString))
                        {
                            foreach (var file in Directory.GetFiles(dataDirectoryPath))
                            {
                                var tableName = Path.GetFileNameWithoutExtension(file);

                                using (
                                    var command =
                                        new SqlCommand(
                                            FormattableString.Invariant($"SELECT * FROM {tableName}"),
                                            connection))
                                {
                                    var fileRecords = File.ReadAllLines(file);
                                    var actualRecords = new List<string>(fileRecords.Length - 1);

                                    connection.Open();
                                    var reader = command.ExecuteReader();

                                    while (reader.Read())
                                    {
                                        actualRecords.Add(string.Join(",", reader));
                                    }

                                    actualRecords.ShouldAllBeEquivalentTo(fileRecords.Skip(1));
                                }
                            }
                        }
                    });
        }
    }
}
