(function () {
    const TWO_PI = Math.PI * 2;
    const round = Math.round;
    const random = (min, max) => {
        if (typeof min === 'undefined') return Math.random();
        if (typeof max === 'undefined') {
            max = min;
            min = 0;
        }
        return Math.random() * (max - min) + min;
    };

    class Particle {
        constructor(x, y) {
            this.reset(x, y);
        }

        reset(x, y) {
            this.x = x + random(-10, 10);
            this.y = y + random(-10, 10);
            this.vx = random(-2.5, 2.5);
            this.vy = random(-5, 5);
            this.radius = random() > 0.75 ? random(25, 50) : 1 + random(1, 20);
            this.lifespan = random(25, 50);
            this.charge = this.lifespan;
            this.color = {
                r: round(random(255)),
                g: round(random(75)),
                b: round(random(50))
            };
            this.opacity = 1;
        }

        update() {
            this.charge -= 1;
            this.radius -= 1;
            this.x += this.vx;
            this.y += this.vy;
        }

        draw(ctx) {
            if (this.radius <= 0) return;

            const gradient = ctx.createRadialGradient(
                this.x,
                this.y,
                0,
                this.x,
                this.y,
                this.radius
            );

            gradient.addColorStop(
                0,
                `rgba(${this.color.r}, ${this.color.g}, ${this.color.b}, ${this.opacity})`
            );
            gradient.addColorStop(
                0.5,
                `rgba(${this.color.r}, ${this.color.g}, ${this.color.b}, ${this.opacity})`
            );
            gradient.addColorStop(
                1,
                `rgba(${this.color.r}, ${this.color.g}, ${this.color.b}, 0)`
            );

            ctx.fillStyle = gradient;
            ctx.beginPath();
            ctx.arc(this.x, this.y, Math.max(this.radius, 0), 0, TWO_PI, false);
            ctx.fill();
        }
    }

    function createParticles(count, x, y) {
        const list = [];
        for (let i = 0; i < count; i++) {
            list.push(new Particle(x, y));
        }
        return list;
    }

    function createFireAnimation(canvas, statsElement) {
        const ctx = canvas.getContext('2d');
        const center = { x: 0, y: 0 };
        const mouse = { x: null, y: null };
        let particles = [];
        let rafId = null;

        function resize() {
            const rect = canvas.getBoundingClientRect();
            canvas.width = rect.width;
            canvas.height = rect.height;
            center.x = canvas.width * 0.5;
            center.y = canvas.height * 0.5;

            if (particles.length === 0) {
                particles = particles.concat(createParticles(100, center.x, center.y));
            }
        }

        function pointerMove(event) {
            const rect = canvas.getBoundingClientRect();
            mouse.x = event.clientX - rect.left;
            mouse.y = event.clientY - rect.top;
        }

        function drawFrame() {
            ctx.globalCompositeOperation = 'source-over';
            ctx.fillStyle = '#0A0B1F';
            ctx.fillRect(0, 0, canvas.width, canvas.height);

            ctx.globalCompositeOperation = 'lighter';

            for (let i = particles.length - 1; i >= 0; i--) {
                const p = particles[i];
                p.opacity = round((p.charge / p.lifespan) * 100) / 100;
                p.draw(ctx);
                p.update();

                if (p.charge < 0 || p.radius < 0) {
                    const originX = mouse.x || center.x;
                    const originY = mouse.y || center.y;
                    particles[i] = new Particle(originX, originY);
                }
            }

            if (statsElement) {
                statsElement.textContent = particles.length.toString();
            }

            rafId = requestAnimationFrame(drawFrame);
        }

        resize();
        window.addEventListener('resize', resize);
        canvas.addEventListener('mousemove', pointerMove);

        rafId = requestAnimationFrame(drawFrame);

        return function cleanup() {
            if (rafId) {
                cancelAnimationFrame(rafId);
                rafId = null;
            }

            window.removeEventListener('resize', resize);
            canvas.removeEventListener('mousemove', pointerMove);
            particles.length = 0;
        };
    }

    window.startFireAnimation = function (canvasId, statsElementId) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error(`Canvas with id "${canvasId}" not found`);
            return () => {};
        }

        const statsElement = statsElementId
            ? document.getElementById(statsElementId)
            : null;

        return createFireAnimation(canvas, statsElement);
    };
})();
