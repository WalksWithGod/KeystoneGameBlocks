<div class = "columns">
	<div id="upperbody">
		<div id="leftcolumn">
		<!--<img src=""height="128" width="128"/> --> <!-- 3d viewport is rendered here.  No image needed.-->
		</div>
		<div id="rightcolumn">
			<h3>$componenttypename$</h3>
			<ul class="details" style="padding-left:0px">
				<li>
					<strong class="title">Spectral Type:</strong>
					<span class="value">$spectraltype$</span>
				</li>
				<li>
					<strong class="title">Luminosity:</strong>
					<span class="value">$luminosity$</span>
				</li>
				<li>
				   <strong class="title">Mass:</strong>
				   <span class="value">$mass$</span>
				</li>
				<li>
					<strong class="title">Mean Density:</strong>
				   <span class="value">$density$</span>
				</li>
				<!--
				Shouldn't this contain more information relevant to player such as hull temperature
				alerts, and escape velocity warning indicators
				-->
				<li>
					<strong class="title">Radius:</strong>
					<span class="value">$radius$</span>
				</li>
				<li>
					<strong class="title">Surface Gravity:</strong>
					<span class="value">$surfacegravity$</span>
				</li>
				<li>
					<strong class="title">Escape Velocity:</strong>
					<span class="value">$escapevelocity$</span>
				</li>
				<li>
				   <strong class="title">Average Surface Temp:</strong>
				   <span class="value">$surfacetemperature$</span>
				</li>
				<li>
					<strong class="title">Surface Radiation:</strong>
				   <span class="value">$surfaceradiation$</span>
			   </li>
			   <li>
					<img id="surveystar" src="images/zoom_grey.png" height="32" width="32" onclick="app.ButtonClick(this.id)"/>
					<img id="detectstar" src="images/satellite_dish.png" height="32" width="32" onclick="app.ButtonClick(this.id)"/>
			   </li>
			</ul>
			</div>
	</div>
   
	<div id ="lowerbody">
					<!--
	<div id="tabs">
		<ul>
		<li><a href="#tabs-1">Orbital Information</a></li>
		</ul>
		<div id="tabs-1">
			<ul class="details" style="padding-left:0px">
				<li>Orbital Velocity - 230 km/s
				<li>Length of Year - 1 Sol Galactic Year = 225 million Earth years
				<li>Enter Orbit - <a href="#" data-handler="GUILayout_ButtonClick">[Low][Middle][GeoSync][GeoStationary][High]</a>
				<li>Change Orbit - <a href="#" data-handler="GUILayout_ButtonClick">[Raise][Lower]</a> 
			</ul>
		</div>
	</div>
	
	New Task I want to look like a "Tab" and with the url shown as Tasks > New Task (or Edit Task)
	E:\My Pictures\_KGB_PICS\manage_engine_activities.png
	E:\My Pictures\_KGB_PICS\manage_engine_new_activity.png
	
	-->

	<div id="tabs">
		<ul>
			<li><a href="#tabs-1">Tasks</a></li>
			<li><a href="#tabs-2">New Task</a></li>
		</ul>
		<div id="tabs-2"> <!-- New Task Form -->
		<caption style="text-align:left;">
		<ul style="padding-left:0px">
			<li><label for="task_opened_by" class="required">Opened By:</label>
			<label id="task_opened_by" value="Hypnotron">Hypnotron</input></li>
			
			<li><label for="task_opened_date" class="required">Opened Date:</label>
			<label id="task_opened_date">NOW</input></li>
			
			<li><label for="task_category" class="required">Category:</label>
			 <select id="task_category" onchange="task_category_onChange(this)">
			   <option value="Survey Astronomical">Survey - Astronomical</option>
			   <option value="Orbit Enter">Oribt - Enter</option>
			   <option value="Orbit Raise">Orbit - Raise</option>
			   <option value="Delete All Records">Delete All Records</option>
			</select></li>
			
			<li><label for="task_category_description" class="required">Description:</label>
			<input id="task_category_description"></input></li>
			
			<li><label for="task_target" class="required">Target:</label>
			<input id="task_target" value="Mercury"></input></li>
			
			<li><label for="task_priority" class="required">Priority:</label>
			<select id="task_priority" onchange="task_priority_onChange(this)">
			   <option value="1">1</option>
			   <option value="2">2</option>
			   <option value="3">3</option>
			   <option value="4">4</option>
			</select></li>
			
			<li><label for="task_assigned_to_department" class="required">Assigned To Department:</label>
			<select id="task_assigned_to_department" onchange="task_assigned_to_department_onChange(this)">
			   <option value="Helm">Helm</option>
			   <option value="Engineering">Engineering</option>
			   <option value="Science">Science</option>
			   <option value="Tactical">Tactical</option>
			</select></li>
			
			<li><label for="task_assigned_to" class="required">Assigned To:</label>
			<input id="task_assigned_to" value="micmanos"></input></li>
			
			<li><label for="task_assigned_to_date" class="required">Assigned To Date:</label>
			<input id="task_assigned_to_date"></input></li>
			
			<li><button id="addtaskOK">Add</button>
			<button id="addtaskCancel"><a href="#tabs-1">Cancel</a></button></li>
		</ul>
		 </caption>
		</div>
		<div id="tabs-1"> <!-- Tasks View -->
			<button id="addtask" onclick="app.ButtonClick(this.id)">Add Task</button>

	<div class="CSSTableGenerator">
		<table id="taskTable" class="tablesorter">
			<thead>
				<tr>
					<th width="5%"></th>
					<th width="5%"></th>
					<th>Name</th>
					<th>Target</th>
					<th>Created By</th>
					<th>Assigned To</th>
					<th>Status</th>
					<th width="10%"></th> <!-- Delete/Suspend/Cancel if in progress-->
				</tr>
			</thead>
			
			<tbody>
				<tr>
					<td><span class="pr3">3</span></td>
					<td><img src="images/steering_wheel.png" height="16" width="16"/></td>
					<td>Travel to</td>
					<td>Venus</td>
					<td>Cpt. Hypnotron</td>
					<td>Helm</td>
					<td>50%</td>
					<td>
						<img id="edittask" src="images/cog_edit.png" height="16" width="16" onclick="app.ButtonClick(this.id)"/>
						<img id="deletetask" src="images/cog_delete.png" height="16" width="16" onclick="app.ButtonClick(this.id)"/>
					</td>
				</tr>
				<tr>
					<td><span class="pr7">7</span></td>
					<td><img src="images/steering_wheel.png" height="16" width="16"/></td>
					<td>Orbit</td>
					<td>Venus</td>
					<td>Cpt. Hypnotron</td>
					<td>Helm</td>
					<td>0%</td>
					<td>
						<img src="images/cog_edit.png" height="16" width="16"/>
						<a href="#" data-handler="GUILayout_ButtonClick"><img src="images/cog_delete.png" height="16" width="16"/></a>
					</td>
				</tr>
				<tr>
					<td><span class="pr9">9</span></td>
					<td><img src="images/flask.png" height="16" width="16"/></td>
					<td>Astronomical Survey</td>
					<td>Venus</td>
					<td>Cpt. Hypnotron</td>
					<td>Science</td>
					<td>0%</td>
					<td>
						<img src="images/cog_edit.png" height="16" width="16"/>
						<a href="#" data-handler="GUILayout_ButtonClick"><img src="images/cog_delete.png" height="16" width="16"/></a>
					</td>
				</tr>
			</tbody>
		</table>
	</div>
	</div>
	<hr>
	<h3>Sattelites [13]
		<a href="#" class="hide">[hide]</a>
		<a href="#" class="show">[show]</a>
	</h3>
	$satellites$
	<!--
	<div class="CSSTableGenerator">
		<table id="sattelitestable" class="tablesorter">
			<thead>
				<tr>
					<th>Name</th>
					<th>Type</th>
					<th>Orbital Distance</th>
					<th>Mass</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>Mercury</td>
					<td>Terrestrial - Rockball</td>
					<td>0.387 AU</td>
					<td>0.055 Earth masses</td>
				</tr>
				<tr>
					<td>Venus</td>
					<td>Terrestrial - Desert</td>
					<td>0.723 AU</td>
					<td>0.815  Earth masses</td>
				</tr>
				<tr>
					<td>Earth</td>
					<td>Terrestrial - Ocean World</td>
					<td>1 AU</td>
					<td>1 Earth masses</td>
				</tr>
				<tr>
					<td>Mars</td>
					<td>Terrestrial - Desert</td>
					<td>1.524 AU</td>
					<td>0.107 Earth masses</td>
				</tr>
				<tr>
					<td>Jupiter</td>
					<td>Gas Giant</td>
					<td>5.203 AU</td>
					<td>317.83 Earth masses</td>
				</tr>
				<tr>
					<td>Saturn</td>
					<td>Gas Giant</td>
					<td>9.539 AU</td>
					<td>95.16 Earth masses</td>
				</tr>
				<tr>
					<td>Uranus</td>
					<td>Gas Giant</td>
					<td>19.182 AU</td>
					<td>14.536 Earth masses</td>
				</tr>
				<tr>
					<td>Neptune</td>
					<td>Gas Giant</td>
					<td>30.058 AU</td>
					<td>17.147 Earth masses</td>
				</tr>
				<tr>
					<td>Pluto</td>
					<td>Dwarf Planet</td>
					<td>39.44 AU</td>
					<td>0.00218 Earth masses</td>
				</tr>
				<tr>
					<td>Eris</td>
					<td>Dwarf Planet</td>
					<td>67.958 AU</td>
					<td>0.0028 Earth masses</td>
				</tr>
				<tr>
					<td>Haumea</td>
					<td>Dwarf Planet</td>
					<td>43.132 AU</td>
					<td>0.00066 Earth masses</td>
				</tr>
				<tr>
					<td>Makemake</td>
					<td>Dwarf Planet</td>
					<td>45.791 AU</td>
					<td>unknown</td>
				</tr>
				<tr>
					<td>Ceres</td>
					<td>Dwarf Planet</td>
					<td>2.7668 AU</td>
					<td>0.00015 Earth masses</td>
				</tr>
			</tbody>
		</table>
	</div>
	<-->
	
	<hr>
	<h3>Artificial Sattelites [9]</h3>
	<ul>
		<li>Defense Laser[155] <a href="#" data-handler="GUILayout_ButtonClick">[Raise][Lower]</a> 
		<li>Shipyard
		<li>Battlecruisers[5]
		<li>Space Station[1]
		<li>Astronomical Telescope[5]
	</ul>
	<hr>
	<h3>Alerts</h3>
	<ul>
		<li>Gravity Well Proximity Warning (0.1 mSv/hour) <a href="#" data-handler="GUILayout_ButtonClick"><img src="radiationicon" alt="caution radiation leak" width="16" height="16"></a>
		<li>Hull Temperature Warning +5% <a href="#" data-handler="GUILayout_ButtonClick"><img src="temperatureicon" alt="caution temperature limit" width="16" height="16"></a>
	</ul>
	<hr>
	</div>
</div>



	