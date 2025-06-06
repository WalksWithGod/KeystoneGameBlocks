var p=10;
function timeout_trigger() 
{

	$('#showProgress').html('<div class="percent"></div><div id="slice"'+(p > 50?' class="gt50"':'')+'><div class="pie"></div>'+(p > 50?'<div class="pie fill"></div>':'')+'</div>');

	var deg = 360/100*p;

	$('#showProgress #slice .pie').css({
	'-moz-transform':'rotate('+deg+'deg)',
	'-webkit-transform':'rotate('+deg+'deg)',
	'-o-transform':'rotate('+deg+'deg)',
	'transform':'rotate('+deg+'deg)'
	});

	$('#showProgress .percent').text(''+p+'%');

	if(p!=100) {
	setTimeout('timeout_trigger()', 50);
	}

	p++;
}

$(function()
{
	timeout_trigger();
	$("#myTable").tablesorter();
	$("#taskTable").tablesorter();
	$("#tabs").tabs();
	
	$("#addtask").bind("click", showEditTaskDialog);
	$("#addtaskOK").bind("click", saveRecord);
	
	/* Trying to add this to our HTML (EVEN WHEN COMMENTED OUT) causes Exception in StringTemplate because of the $() i suspect.
	<script type='text/javascript'>
		$(function() {
			timeout_trigger();
			$("#myTable").tablesorter();			

		});	
	</script>
	*/
		
});

function showEditTaskDialog()
{
    $("#dialog-modal").dialog(
    {
        width: 300,
        height: 200,
        open: function(event, ui)
        {
            var textarea = $('<textarea style="height: 276px;">');
            $(textarea).redactor({
                focus: true,
                autoresize: false,
                initCallback: function()
                {
                    this.set('<p>Lorem...</p>');
                }
            });
        }
     });
}