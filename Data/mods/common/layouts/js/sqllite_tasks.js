
/****************************************************************************
*Purpose of the document is to explain below functionalities of HTML5 offline data storage
1. Save Record
2. Access Record
3. Delete Record
4. Edit & Update Record
****************************************************************************/
 
/****************************************************************************
Global variables
****************************************************************************/
var Type; var Url; var Data; var ContentType; var DataType; var ProcessData;
var db = window.openDatabase("tasks", "1.0", "TasktDatabase", 1024 * 1000);
var tds;
var selectedRecordsArr = [];
var recordUpdateFlag = false;
var btnSubmit_SaveLabel = "Save Record";
var btnSubmit_UpdateLabel = "Update Record";
var recordIdToUpdate;

/****************************************************************************
*Add event listeners for buttons - Save Record / Update Record button
****************************************************************************/
$(document).ready(function ()
{
	// $('footer').text( "The DOM is now loaded and can be manipulated." );
	 
     // db.transaction(function (tx) {tx.executeSql('CREATE TABLE IF NOT EXISTS Tasks(Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, seqId INTEGER, name TEXT NOT NULL, emailId TEXT NOT NULL, telNo TEXT NOT NULL, city TEXT NOT NULL)', []);});
     // $('#btnSubmit').bind('click', function ()
     // {
		// saveRecord();
     // });
    
	renderRecordsOnScreen();
});
 
 
/****************************************************************************
Function: comboAction_onChange()
Purpose: to select all records of the table if action selected is 'Delete All Records'
****************************************************************************/
function comboAction_onChange(actionList)
{
   var recordCount = $("#taskTable > tbody > tr").length;
   if (actionList.value == 'Delete All Records' && recordCount > 0)
   {
      $("#chkSelectAll").prop("checked", true);
      selectAllRecords();
   }
   else if (recordCount == 0)
   {
     comboAction.selectedIndex = 0;
     alert('No record available/selected to perform action.');
   }//end of else if
}//end of function
 
/****************************************************************************
Function: actionOnSelectedRecord()
Purpose: select action to be performed on single or group of records & click OK to commit.
Flow: select action from combo box to be performed on single or group of records & click OK to commit.
****************************************************************************/
function actionOnBulkRecords()
{
   var actionToPerform = comboAction.value; //value at selectedIndex;
   var selectedRecordsCount = selectedRecordsArr.length;
   if (selectedRecordsCount > 0)
   {
     switch (actionToPerform)
     {
        case "Edit Record":
            {
             //alert('to edit -: ' + selectedRecordsArr.toString());
             editRecord();
             break;
            }//end of case "Edit Record":
        case "Delete Record":
            {
             //alert('to delete -: '+ selectedRecordsArr.toString())
             deleteRecords();
             break;
            }//end of case "Delete Record"
        case "Delete All Records":
            {
              //alert('Delete all')
              deleteRecords();
              break;
            } //end of case "Delete All Records"
        default:
            {
              alert('no action..');
              break;
            }
       }//end of switch block
   }
   else
   {
      alert('Please select atleast one record..');
   }
}//end of function
 
/****************************************************************************
Function: saveRecord()
Purpose: To save the entered details into offline database
Flow:
1. It restrics to enter all the fields on the form.
2. creates table (Tasks) if not exist.
3. creates Task object and pass it to insertTask() method to save the record
****************************************************************************/
function saveRecord()
{
	// todo: #task_source
    if ($('#task_opened_by').val() == "" || 
	    $('#task_opened_date').val() == "" || 
		$('#task_category').val() == "" || 
		$('#task_target').val() == "" || 
		$('#task_assigned_to_department').val() == "" || 
		$('#task_priority').val() == "")
    {
       alert("All fields are mandatory");
    }//end if
    else
    {
	
	// TODO: Ideally, this should call a command in EXE to send network command CreateTask()
	//       so that we can do proper validation server side of the command's parameters
	//       javascript should only be good for READing the db, not for WRITEs.
	//       So we need for QuickLook to be notified when "CreateTask" button is called
	
			// todo: what if category, sub-category, sub-sub category combine to give us a 10000 + 1000 + 100 digit code?
			// eg. Command = 10,000 
			//     Helm = 20,000
			//	   Science = 30,000
			//	   Tactical = 40,000
			//     AwayTeam = 60,000
		//taskDetails = {}
		
		userDetails = { seqId: 1, 
						name: $('#Full_Name').val(),
						email: $('#Email_Address').val(),
						tel: $('#Telephone_Number').val(),
						city: $('#City').val() };
					 
		// insert new record
		if (btnSubmit.value == btnSubmit_SaveLabel)
		{
			insertRecord(userDetails["name"], 
						 userDetails["email"],
						 userDetails["tel"], 
						 userDetails["city"]);
		}
		// update existing record
		else if (btnSubmit.value == btnSubmit_UpdateLabel)
		{
			updateRecord(userDetails["name"], 
						 userDetails["email"],
						 userDetails["tel"], 
						 userDetails["city"],
						 recordIdToUpdate);
		}
   } //End of else statement
}
/****************************************************************************
Function: insertRecord()
Purpose: To save the entered details into offline database
Flow: inserts the passed details in to the HTML table (taskTable)
****************************************************************************/
function insertRecord(commandCode, commandSource, commandTarget, openedBy, openedDate, assignedTo, priority)
{
	db.transaction(function (tx)
	{
		tx.executeSql('INSERT INTO Tasks(command_code, command_source, command_target, opened_by, opened_date, assigned_to, priority) VALUES (?, ?, ?, ?, ?, ?, ?)', [commandCode, commandSource, commandTarget, openedBy, openedDate,assignedTo, priority],
			insertRecord_onSuccess, insertRecord_onError);
	});
}

// as a commander, you can review tasks to see if any of your officers are abusing their positions
// and maybe you punish or maybe you ignore it because you cannot afford to lose them
/* 
	CurrentTasks table
    TaskHistory table
	CurrentRequests table
	RequestHistory table
*/

/* 
	RequestRecord  {requests can be withdrawn via a withrawrequest request!}
	id
	request_source 
	active {bool for open/closed accepted/pending/rejected)
	resolution {approved, rejected}
	reason     { insufficient funds, target out of range, target not on world/in present system }
	
*/

/* 
	TODO: top "Sol" stats should show relevant to us
		- Distance Away
		- #of unexplored worlds
		- # of worlds 
		- # of uknown worlds
		- Alerts when applicable
		- Notes:
		- a button to retreive a history of tasks there, filterable (eg system enter/leave events only)
		
	-TaskRecord
	id
	command_code (using a code to string description lookup in long term but perhaps just using a readable string in the mean time eg. "navigate_to")
	command_source // vehicleID
	command_target // todo: should we use a target_type so we can narrow down which xml?  should command string be single task_string element and contain both command_code and arguments and be parseable by whatever is assigned the task?
	opened_by      // user's avatar rank + name, crew rank + name, or computer name + processID
	opened_date
	assigned_to    //"auto", department/rank, user's avatar name or rank, crew name, or computer name + processID
	assignment_history // if crew is unable or unable to perform/resume/accept a task, the task will be reassigned
	priority 1 - 10
	active { bool for open/closed }
	resolution { completed, postponed,} + reason tied in
	
	
	status_history 
	{
		id
		taskID
		date_time
		support { the list of station\components\items\slots used to fascilitate the execution of this task.  eg. forward_sensor_array_#1_process_#1_of_10}
		action
		performed_by
		duration {time in seconds performing this action}
		resolution
	}
	
	time_reqt {estimate of total seconds required to complete task) 
	projected_start - The date when the task is scheduled to start
	projected_end   - The date when the task is suppose to end
	actual_start    - The date when the task was started
	actual_end	    - The date when the task was ended
	last_update     - The date when the task was last updated


*/

/****************************************************************************
Function: insertRecord_onSuccess() / insertRecord_onError()
Purpose: insert record event handlers
Flow:
****************************************************************************/
function insertRecord_onSuccess(tx, result)
{
  refreshHtmlPage(); //delete existing records on the screen if any
  renderRecordsOnScreen(); //fetch records from offline db & render on the screen
  resetDataEntryForm();
}
 
function insertRecord_onError(tx, error)
{
  alert(error.message);
}
/****************************************************************************
Function: updateRecord()
Purpose: To update the edited details into offline database
Flow:
****************************************************************************/
function updateRecord(varName, varEmail, varTel, varCity, varRecordId)
{
   var updateQuery = "UPDATE Tasks SET name = ?, emailId = ?, telNo = ?, city = ? WHERE Id=?";
   db.transaction(function (tx)
   {
		tx.executeSql(updateQuery, [varName, varEmail, varTel, varCity, varRecordId],
                   updateRecord_onSuccess, updateRecord_onError);
   });
}
 
/****************************************************************************
Function: updateRecord_onSuccess() / updateRecord_onError()
Purpose: update record event handlers
Flow:
****************************************************************************/
function updateRecord_onSuccess(tx, result)
{
	refreshHtmlPage(); //delete existing records on the screen if any
	renderRecordsOnScreen(); //fetch records from offline db & render on the screen
	resetDataEntryForm();
}
function updateRecord_onError(tx, error)
{
	alert(error.message);
}
/****************************************************************************
Function: resetDataEntryForm()
Purpose: to reset all the fields of data entry form
Flow:
****************************************************************************/
function resetDataEntryForm()
{
	$('#Full_Name').val("");
	$('#Email_Address').val("");
	$('#Telephone_Number').val("");
	$('#City').val("");
}
/****************************************************************************
Function: renderRecordsOnScreen()
Purpose: fetch all records one by one from offline database(Sqlite) & pass it to addTableRow() method
for displaying on the screen
Flow:
****************************************************************************/
function renderRecordsOnScreen()
{
	return;	   
   query = "SELECT t.Id, t.priority, depts.name as departmentname, codes.name AS commandname, ent.name AS targetname, crew.name AS openedby, crew2.name AS assignedto FROM Tasks t INNER JOIN TaskCodes codes ON t.command_code = codes.Id INNER JOIN CelestialEntities ent ON t.command_target = ent.Id INNER JOIN Crew crew ON t.opened_by = crew.Id INNER JOIN Crew crew2 ON t.assigned_to = crew2.Id INNER JOIN TaskDepartments depts ON codes.departmentID = depts.Id";
	   
	db.transaction(function (tx) {
		tx.executeSql(query, [],
		
	   function (tx, rs)
	   {
		 if (rs.rows.length > 0)
		 {
		   for (var i = 0; i < rs.rows.length; i++)
		   {
				r = rs.rows.item(i);
				// tds = '<tr><td><input type="checkbox" id="' + 
					// r['Id'] + '" onclick="selectRecord(this)" />' + 'Chk-' + i + '</td><td>' + 
				    // r['name'] + '</td>' + '<td>' +
					// r['emailId'] + '</td>' + '<td>' + 
					// r['telNo'] + '</td>' + '<td>' + 
					// r['city'] + '</td></tr>';
			 
			 /*<tr>
					<td><span class="pr3">3</span></td>
					<td><img src="images/steering_wheel.png" height="16" width="16"/></td>
					<td>Travel to</td>
					<td>Venus</td>
					<td>Cpt. Hypnotron</td>
					<td>Helm</td>
					<td>50%</td>
					<td>
						<img src="images/cog_edit.png" height="16" width="16"/>
						<img src="images/cog_delete.png" height="16" width="16"/>
					</td>
				</tr>
			*/
			
			tds = '<tr><td><span class="pr' + r['priority'] + '">' + r['priority'] + '</span></td>' +
					'<td><img src="images/steering_wheel.png" height="16" width="16"/></td>' +
					'<td>' + r['commandname'] + '</td>' +
					'<td>' + r['targetname'] + '</td>' +
					'<td>' + r['openedby'] + '</td>' +
					'<td>' + r['assignedto'] + '</td>' +
					'<td>50%</td>' +
					'<td>' +
						'<img src="images/cog_edit.png" height="16" width="16"/>' +
						'<img src="images/cog_delete.png" height="16" width="16"/>' +
					'</td>'
					'</tr>';
				
			 addTableRow($('#tasktable'));
		  }//end of for
		 }
	   },
	   function (tx, error)
	   {
			alert(error.message);
			//alert('error while rendering records on screen..');
	   });
   });
}
 
/****************************************************************************
Function: refreshHtmlPage()
Purpose: refresh the html page & reset the global variables
Flow:
****************************************************************************/
function refreshHtmlPage()
{
	var rowCount = $("#taskTable > tbody > tr").length;
	if (rowCount > 0) { $("tbody", tblRecords).remove(); }
	$("#chkSelectAll").prop("checked", false);
	btnSubmit.value = btnSubmit_SaveLabel;
	comboAction.selectedIndex = 0;
	recordIdToUpdate = null;
	selectedRecordsArr = [];
}
/****************************************************************************
Function: addTableRow()
Purpose: display the record fetched from offline db.
Flow:
****************************************************************************/
function addTableRow(jQtable)
{
  jQtable.each
  (
    function ()
    {
		var $table = $(this);
		if ($('tbody', this).length > 0)
		{
			$('tbody', this).append(tds);
		} else {
		  $(this).append(tds);
		}
    }
  ); //end of jQtable.each
}
 
/****************************************************************************
Function: selectAllRecords()
Purpose: function called on click of checkbox in the table header.
Flow:
****************************************************************************/
function selectAllRecords()
{
	var recordCount = $("#taskTable > tbody > tr").length;
	selectedRecordsArr = []; //initially reset the array to blank
	//if ($('#chkSelectAll').is(':checked') == true && recordCount > 0)
	if (recordCount > 0)
	{
		$("#taskTable > tbody > tr").each(function () //for each <tr> element that is child of table "#taskTable > tbody"
		{
			//td > * means get all children of the <td> element & represent it by the "this" object
			$("td > *", this).each(function () {
				if ($(this).attr("type") == "checkbox")
				{
					this.checked = $('#chkSelectAll').is(':checked');
					if ($('#chkSelectAll').is(':checked')) //insert checkbox id into array only if header chkbox is selected
					{
						selectedRecordsArr.push(this.id) //id of the checkboxes
					}
				}
			});
		});
	}
}
 
/****************************************************************************
Function: selectRecord(chkbox)
Purpose: function called on click of checkbox in each row.
Flow:
****************************************************************************/
function selectRecord(chkbox)
{
var recordId = chkbox.id;
selectedRecordsArr = []; //initially reset the array to blank
var recordCount = $("#taskTable > tbody > tr").length;
if (recordCount > 0)
{
	$("#taskTable > tbody > tr").each(function () //for each <tr> element that is child of table "#taskTable > tbody"
	{
		//td > * means get all children of the <td> element & represent it by the "this" object
		$("td > *", this).each(function () {
			//use the type attribute to find out exactly what type of input element it is - text, checkbox, button, etc..
			if ($(this).attr("type") == "checkbox" && ($(this).is(':checked')) == true)
			{
				var x = this.id;
				selectedRecordsArr.push(this.id) //id of the selected checkboxes
			}
			});
		});
	}
//alert(selectedRecordsArr.toString());
}
/****************************************************************************
Function: editRecord()
Purpose: edit single record
Flow:
****************************************************************************/
function editRecord()
{
//var updateQuery = 'UPDATE Tasks';
if (selectedRecordsArr.length > 1) {
	alert('You can select only one record to edit at a single time.');
}else {
	var recordId = selectedRecordsArr.toString();
	var selectQuery = 'SELECT * FROM Tasks Where Id = ' + recordId;

	db.transaction(function (tx) {
		tx.executeSql(selectQuery, [], function (tx, rs) {
			if (rs.rows.length == 1)
			{
				r = rs.rows.item(0);
				recordIdToUpdate = r['Id'];
				Full_Name.value = r['name']
				Email_Address.value = r['emailId'];
				Telephone_Number.value = r['telNo'];
				City.value = r['city'];
				recordUpdateFlag = true;
				btnSubmit.value = btnSubmit_UpdateLabel;
			}
			}, function (tx, error) { alert('error while fetching record to edit..'); });
		});
	}
}
 
/****************************************************************************
Function: deleteRecords()
Purpose: delete all rows from offline database
Flow:
****************************************************************************/
function deleteRecords()
{
	var deleteQuery; 
	if ($('#chkSelectAll').is(':checked')) //delte all records
		deleteQuery = 'DELETE FROM Tasks';
	else
		deleteQuery = 'DELETE FROM Tasks where Id IN(' + selectedRecordsArr.toString() + ')';
  
	db.transaction(function (tx){tx.executeSql(deleteQuery, [], deleteRecord_onSuccess, deleteRecord_onError)});
}
 
function deleteRecord_onSuccess(tx, result) //delete record transaction success handler
{
	refreshHtmlPage(); //delete existing records on the screen if any
	renderRecordsOnScreen(); //fetch records from offline db if any & render on the screen
}

function deleteRecord_onError(tx, error) //delete record transaction failure handler
{
	alert('error while deleting records');
}