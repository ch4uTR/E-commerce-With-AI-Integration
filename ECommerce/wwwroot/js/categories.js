document.addEventListener("DOMContentLoaded", function () {
    fetch("/api/CategoryAPI")  // senin API route'un
        .then(response => response.json())
        .then(categories => {
            const slider = document.querySelector(".categories__slider");
            slider.innerHTML = ""; // eski statik öğeleri temizle

            categories.forEach(cat => {
                const div = document.createElement("div");
                div.classList.add("col-lg-3");
                div.innerHTML = `
                    <div class="categories__item set-bg" data-setbg="${cat.imageUrl}">
                        <h5><a href="#">${cat.name}</a></h5>
                    </div>
                `;
                slider.appendChild(div);
            });

            // Slider'ı tekrar başlat (Owl Carousel kullanıyorsan)
            $(".categories__slider").owlCarousel({
                loop: true,
                margin: 0,
                items: 4,
                dots: false,
                nav: true,
                navText: ["<span class='fa fa-angle-left'><span/>", "<span class='fa fa-angle-right'><span/>"],
                animateOut: 'fadeOut',
                animateIn: 'fadeIn',
                smartSpeed: 1200,
                autoHeight: false,
                autoplay: true,
                responsive: {
                    0: { items: 1 },
                    480: { items: 2 },
                    768: { items: 3 },
                    992: { items: 4 }
                }
            });

            // set-bg tekrar çalıştır
            $('.set-bg').each(function () {
                var bg = $(this).data('setbg');
                $(this).css('background-image', 'url(' + bg + ')');
            });

        })
        .catch(err => console.error(err));
});
