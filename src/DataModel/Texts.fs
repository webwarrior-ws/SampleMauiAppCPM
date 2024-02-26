namespace DataModel

open System

// FIXME: maybe this should be extracted to a different lib, kess related to the "Model"
module Texts =
    let AppName = "RunIntoMe"
    let private EOL = Environment.NewLine

    let ToUseAppAccessToYourLocationIsRequired =
        sprintf
            "To use %s, access to your GPS location is required at all times, so that you can find out how close you and your friends are and arrange meetups.%sYour location data will only be used in real-time and will not be recorded. For added privacy, your friends will not know who you are or see your location until a meetup is agreed. Please grant background location permission and start using %s today!"
            AppName
            EOL
            AppName

    let GpsLocationFeatureIsNeeded =
        sprintf
            "%s needs your GPS location feature enabled in order to run."
            AppName

    let BackgroundLocationPermissionIsNeeded =
        sprintf
            "%s needs your background location to run.%sGo to settings on your device and enable it."
            AppName
            EOL

    let Success = "Success!"
    let SuccessRelationshipMsg = "You and {0} are now connected!"
    let Note = "Note"
    let RelationshipAlreadyExistsMsg = "You were already connected!"
    let Ok = "Ok"
    let AddFolk = "Add folk"
    let ShowQR = "Show QR"
    let ScanQR = "Scan QR"
    let NewNotification = "New notification"
    let Closeness = "Closeness"
    let Acquaintance = "Acquaintance"
    let Comrade = "Comrade"
    let Friend = "Friend"
    let Family = "Family"
    let Regular = "Regular"
    let Close = "Close"
    let Crush = "Crush"
    let Relationships = "Connections"
    let Loading = "Loading..."
    let NoFolksAddedYet = "No folks added yet"
    let Status = "Status:"
    let Busy = "Busy"
    let Free = "Free"
    let ViewRelationships = "View connections"
    let CurrentCoordinatesNA = "Current coordinates: N/A"

    let CantGetGPSLocationBecauseOfRunningInWindows =
        "Can't get GPS location because of running in Windows"

    let GpsLocationInfoBegin = "GPS location...?"

    let GpsLocationInfo =
        "{0}: location was
        {1} {2}"

    let Warning = "Warning"
    let GivePermissionAndStartTheApp = "Give permission and start the app"
    let BeforeYouStart = "Before you start"
    let OpenSettings = "Open settings"
    let YourNickname = "Your nickname"
    let YouDidntSetYourNickname = "You didn't set your nickname. Please do it!"
    let FolkIsNearby = "Some folk is close to you!"
    let FolkWasNearby = "Some folk was close to your current location!"
    let Alert = "Alert!"
