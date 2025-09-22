
next.addEventListener('click', function () {
	var cls = trak.className.split('-').pop();
	cls > 6 ? cls = 7 : cls++;

	step.innerHTML = cls;
	trak.className = 'progress-' + cls;
});

prev.addEventListener('click', function () {
	var cls = trak.className.split('-').pop();
	cls < 1 ? cls = 0 : cls--;

	step.innerHTML = cls;
	trak.className = 'progress-' + cls;
});