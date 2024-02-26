using System;
using Frontend.Models;

namespace Frontend.Services;

public interface IRelationshipsService
{
    List<Folk> GetUserListOfFolks();
    void AddFolk(Folk folk);
    void UpdateRelationshipsInFile(Dictionary<int, int> folks);
}

