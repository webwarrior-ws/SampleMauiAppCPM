using System;
using System.Text.Json;
using Frontend.Models;

namespace Frontend.Services;

public class RelationshipsService : IRelationshipsService
{
    FileInfo relationshipsFile = new FileInfo(Path.Combine(FileSystem.AppDataDirectory, "relationships.json"));

    public List<Folk> GetUserListOfFolks()
    {
        if (!relationshipsFile.Exists)
            return new List<Folk>();

        string listOfFolksJSON = File.ReadAllText(relationshipsFile.FullName);
        return JsonSerializer.Deserialize<List<Folk>>(listOfFolksJSON);
    }

    public void AddFolk(Folk folk)
    {
        var folks = GetUserListOfFolks();
        folks.Add(folk);
        File.WriteAllText(relationshipsFile.FullName, JsonSerializer.Serialize<List<Folk>>(folks));
    }

    public void UpdateRelationshipsInFile(Dictionary<int, int> folksInDatabase)
    {
        var folksInFile = GetUserListOfFolks();
        folksInFile.RemoveAll(Folk => !folksInDatabase.ContainsKey(Folk.ID));

        foreach (var id in folksInDatabase.Keys)
        {
            if (!folksInFile.Any(Folk => Folk.ID == id))
                folksInFile.Add(new Folk() { ID = id, Nickname = string.Empty });
        }

        File.WriteAllText(relationshipsFile.FullName, JsonSerializer.Serialize<List<Folk>>(folksInFile));
    }
}

