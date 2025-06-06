<div class = "columns">
  <div id="level0">
	<div id="componenticon">
		<img src="reactor1_quicklook.png"height="128" width="128"/>
	</div>
    <div id="level1">
      <div id="level2">
          <div id="main">
			<h3>$componenttypename$</h3>
			<ul class="details" style="padding-left:0px">
				<li>
					<strong class="title">Load:</strong>
					<span class="value">$poweruse_over_powergenerated$</span>
				</li>
				<li>
					<strong class="title">Condition:</strong>
					<span class="value">Fully Operational</span>
				</li>
				<li>
				   <strong class="title">Core Temperature:</strong>
				   <span class="value">833C</span>
				</li>
			   <li>
					<strong class="title">Coolant Pressure:</strong>
				   <span class="value">139 bar</span>
			   </li>
				<li>
					<strong class="title">Radiation Leak:</strong>
					<span class="value">0.1 mSv/hour</span><a href="#" data-handler="GUILayout_ButtonClick"><img src="radiationicon" alt="caution radiation leak" width="16" height="16"/>[Cancel Repair][Repair]</a>
				</li>
			</ul>
             </div>
           </div>
       </div>
   </div>
	<div id="operations">
		<hr>
		<h3>Operations</h3>
		<ul>
			<li>Main Breaker - On [Turn Off]</li>
			<li>Regulator - 100%</li>
			<li>Uranium-235 (internal) [replenish][remove]</li>
		</ul>
		<hr>
		<h3>Output</h3>
		<ul>
			<li>Channel[1] 75,000 W</li>
			<li>Channel[2] 75,000 W</li>
		</ul>
		<hr>
		<h3>Channels</h3>
		<a href="#" class="hide">[hide]</a>
		<a href="#" class="show">[show]</a>
		$consumertable$
		<hr>
		<h3>Maintenance</h3>
		<ul>
			<li>Core Radiation Leak - [ 10 supply, ETA .2 Hr]  <a href="#" data-handler="GUILayout_ButtonClick"><img src="wrenchicon" alt="repair radiation leak" width="16" height="16"/></a></li>
			<li>Plasma Coil[1] - [ 1 supply, ETA .5 Hr] <a href="#" data-handler="GUILayout_ButtonClick"><img src="wrenchicon" alt="repair plasma coil" width="16" height="16"/></a></li>
			<li>Pressure Manifold[1] - [ ETA .5 Hr] <a href="#" data-handler="GUILayout_ButtonClick"><img src="wrenchicon" alt="repair plasma coil" width="16" height="16"/></a></li>
			<li>Compression Coil[3] - [ ETA .5 Hr] <a href="#" data-handler="GUILayout_ButtonClick"><img src="wrenchicon" alt="repair plasma coil" width="16" height="16"/></a></li>
			<li>Induction Field Generator[1] - [ ETA .5 Hr] <a href="#" data-handler="GUILayout_ButtonClick"><img src="wrenchicon" alt="repair plasma injector" width="16" height="16"/></a></li> 
			<li>Catalytic Converter[1] - [ ETA .5 Hr] <a href="#" data-handler="GUILayout_ButtonClick"><img src="wrenchicon" alt="repair plasma coil" width="16" height="16"/></a></li>
			<li>Electronic Relays[7] - [ ETA .5 Hr] <a href="#" data-handler="GUILayout_ButtonClick"><img src="wrenchicon" alt="repair plasma coil" width="16" height="16"/></a></li>
			<li>Coolant Leak - [ ETA .5 Hr] <a href="#" data-handler="GUILayout_ButtonClick"><img src="wrenchicon" alt="repair coolant leak" width="16" height="16"/></a></li> 
			<li><a href="#" data-handler="GUILayout_ButtonClick"><img src="wrenchicon" alt="repair overhaul" width="16" height="16"/>[Overhaul]</a> <a href="#" data-handler="GUILayout_ButtonClick"><img src="wrenchicon2" alt="jettison core" width="16" height="16"/>[Jettison Core]</a></li> 
		</ul>
		<hr>
		<h3>Alerts</h3>
		<ul>
			<li>Radiation Leak (0.1 mSv/hour) <a href="#" data-handler="GUILayout_ButtonClick"><img src="radiationicon" alt="caution radiation leak" width="16" height="16"/></a></li>
			<li>Core Temperature Limit +5% <a href="#" data-handler="GUILayout_ButtonClick"><img src="temperatureicon" alt="caution temperature limit" width="16" height="16"/></a></li>
			<li>Coolant Pressure - 75%</li>
			</ul>
		<hr>
		<h3>Access</h3>
		<ul>
			<!-- I need to be able to switch between locked and unlocked per item as appropriate -->
			<li>Interface Termainal Lock Engaged <a href="#" data-handler="GUILayout_ButtonClick"><img src="lockicon" alt="lock interface terminal" width="16" height="16"/></a></li>
			<li>Service Panel[1] Lock Engaged <a href="#" data-handler="GUILayout_ButtonClick"><img src="lockicon" alt="lock interface terminal" width="16" height="16"/></a></li>
			<li>Service Panel[2] Lock Engaged <a href="#" data-handler="GUILayout_ButtonClick"><img src="lockicon" alt="lock interface terminal" width="16" height="16"/></a></li>
		</ul>
	</div>
</div>