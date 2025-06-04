var i=9, fX=document.getElementById("fX"+i);
while (fX != undefined)
{
	var text = fX.innerHTML;
	fX.innerHTML = text;
	i--;
	fX=document.getElementById("fX"+i);
}