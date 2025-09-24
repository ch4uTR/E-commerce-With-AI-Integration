function svgturkiyeharitasi() {
    const element = document.querySelector('#svg-turkiye-haritasi');
    const info = document.querySelector('.il-isimleri');
    let aktifIl = null;

    element.addEventListener(
        'mouseover',
        function (event) {
            if (event.target.tagName === 'path') {
                const ilAdi = event.target.parentNode.getAttribute('data-iladi');
                if (aktifIl !== ilAdi) {
                    info.innerHTML = [
                        '<div>',
                        event.target.parentNode.getAttribute('data-iladi'),
                        '<p>Buraya eklemeler yaptım</p>',
                        '</div>'
                    ].join('');
                    aktifIl = ilAdi;

                }                   
            }
        }
    );

    element.addEventListener('mousemove', function (event) {
        const rect = element.getBoundingClientRect(); // SVG container’ın koordinatları
        info.style.top = (event.clientY - rect.top + 10) + 'px';
        info.style.left = (event.clientX - rect.left + 10) + 'px';
    });

    element.addEventListener(
        'mouseout',
        function (event) {
            info.innerHTML = '';
            aktifIl = null;
        }
    );

    element.addEventListener(
        'click',
        function (event) {
            if (event.target.tagName === 'path') {
                const parent = event.target.parentNode;
                const id = parent.getAttribute('id');

                window.location.href = (
                    '#'
                    + id
                    + '-'
                    + parent.getAttribute('data-plakakodu')
                );
            }
        }
    );
}

svgturkiyeharitasi();
