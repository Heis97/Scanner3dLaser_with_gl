///////////////////////////////////////
// Values of these parameters can be edited

const anglesMethod = "BY_ANGLES_LIST"; // "BY_TOTAL_NUMBER" - the number of positions is specified; "BY_ANGLES_LIST" - custom angle values are specified
const returnTableToStartPosition = true; // true or false
const NoMarkers = true; // script can be run in projects without markers
const WithMarkers = false; // script can be run in projects with markers
const RotationTable = false; // script can be run in projects with a turntable
const WithMarkersOnTable = false; // script can be run in projects with turntable and markers
const Metrological = true; // script can be run in metrology projects
const ProcessingMode = true; // script can be run in processing mode

///////////////////////////////////////
// The following text can not be edited by the user

let newScans = new Array();

function ProcessEvent(module, event, value)
{
	Application.LogMessage("event module = " + module + ", event = " + event+", value = " + JSON.stringify(value));
	if( module === "Script" )
	{
		if ( event === "OnStarted" )
		{
			CheckFeasibility();
			//MakeScan();
			
			Application.LogMessage("CheckFeasibility");


           // FinishScript();
		}
	}

    if( module === "Application" )
	{
		if (event === "OnCheckPassed") {
			//  Write here what a script should do when started
			Application.LogMessage("Script started");
			/*let mes = "{\"module\":\"Scan\",\"command\":\"Create\",\"value\":[\"asd\"]}";
			let back = "\\";
			//mes = mes.replace(back, "");
			//let test_com = "{"module":"Scan","command":"Create","value":["asd"]}"
			let test_com2 = new Command("Scan", "create", ['asd']);
			//Application.LogMessage(mes);
			var res = JSON.parse(JSON.stringify(test_com2));
			var res = JSON.parse(mes);
			//Application.LogMessage("mes0: " +mes["module"]);

			Application.LogMessage("test: " + JSON.stringify(test_com2));
			Application.LogMessage(mes);
			*/
			//StartScanCreation();
			//FinishScript();
			TcpServer.Start(31000);
		}
		else if ( event === "OnModelBuilt" )
		{

			Application.LogMessage("model built");
			TcpServer.SendCommand("model built");
			//FinishScript();
		}
       else if ( event === "OnScanCreated" )
       {
            let newScanCount = value["newScanCount"];
			if (value["newScanCount"] == 1)
			{
				let newScan = value["newScanName"];
				newScans.push(newScan);
			}
			Application.LogMessage("scan created");
			TcpServer.SendCommand("scan created");
			//BuildModel();
       }
		else if ( event === "OnNoiseRemoved" )
		{
			Application.LogMessage("Noise removed");
			newScans = value["scans"];
		}
		else if ( event === "OnGloballyRegistred" )
		{
			Application.LogMessage("Global registration completed");
		}
		else if ( event === "OnScanningStopped" )
		{
			Application.LogMessage("Scanning stopped by user");
			Application.ScriptFinished();
		}
		else if ( event === "OnNoiseRemovalStopped" )
		{
			Application.LogMessage("Noise removal stopped by user");
			Application.ScriptFinished();
		}
		else if ( event === "OnGlobalRegistrationStopped" )
		{
			Application.LogMessage("Global registration stopped by user");
			Application.ScriptFinished();
		}
		else if ( event === "OnModelBuildingStopped" )
		{
			Application.LogMessage("Model building stopped by user");
			Application.ScriptFinished();
		}
		else if ( event === "OnTableNotFound" )
		{
			Application.LogMessage("Turntable not found");
			Application.ScriptFinished();
		}
		else if ( event === "OnIncorrectProjectType" )
		{
			Application.LogMessage("Error! Script could not be run due to unsuitable command or project type.");
			Application.ScriptFinished();
		}
		else if ( event === "OnTryToScanInProcessingMode" )
		{
			Application.LogMessage("Script could not be run because scanning is not available in processing mode. Create a new project suitable for the script type!");
			Application.ScriptFinished();
		}
		
		else if ( event === "OnCheckFailed" )
		{
			Application.LogMessage("Error! Script could not be run due to unsuitable command or project type");
			Application.ScriptFinished();
		}
    }
    if (module === "TcpServer")
	{
		if (event === "ClientConnected")
		{
			Application.LogMessage("TCP-client connected");
		}
		else if (event === "ClientDisconnected")
		{
			Application.LogMessage("TCP-client disconnected");
		}
		
		else if (event === "OnEvent")
		{
			Application.LogMessage("TcpServer::CommandResult: value = " + JSON.stringify(value));
			let response = JSON.parse(value);
			if (response["value"] == "clear") {
				newScans = new Array();
			}
			else {
				Application.LogMessage("section = " + response["module"] + ", command = " + response["command"] + ", value = " + response["value"]);
				//Application.ExecuteCommand(response["module"], response["command"], response["value"]);
				Application.ExecuteCommand(response["module"], response["command"], newScans);
            }
			
			
		}
	}

}


class Command {
	constructor(module1, command1, value1) {
		this.module1 = module1;
		this.command1 = command1;
		this.value1 = value1;
	}
}

function CheckFeasibility()
{
	let flags = 0;
	flags |= NoMarkers;
	flags |= WithMarkers << 1;
	flags |= RotationTable << 2;
	flags |= WithMarkersOnTable << 3;
	flags |= Metrological << 4;
	flags |= ProcessingMode << 5;
	Application.ExecuteCommand("General", "CheckFeasibility", {projectType: flags});
}
function FinishScript()
{
	Application.LogMessage("Script finished");
	Application.ScriptFinished();
}

function BuildModel()
{
	Application.LogMessage("Model building started");
	Application.ExecuteCommand("Processing", "BuildModel", {scans: newScans});
}

function MakeScan()
{
	Application.LogMessage("Scanning started");
	Application.ExecuteCommand("Scan", "Create");
}

function RotateScan(scanName)
{
	let angleAbsolute = GetAbsoluteAngle();
	Application.LogMessage("Scan rotation");
	Application.ExecuteCommand("Scan", "Rotate", {angle: -angleAbsolute, scan: scanName});
}


