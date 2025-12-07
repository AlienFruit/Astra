(function() {
    console.log('Библиотека огня загружена из внешнего файла fire.js!');

    // Глобальные переменные для работы с огнем
    window.fireFlames = [];
    window.fireSparks = [];
    window.fireTime = 0;
    window.fireAnimationId = null;
    window.fireResizeHandler = null;

    // Функция очистки всех ресурсов
    window.fireCleanup = function() {
        console.log('Очистка ресурсов огня...');

        if (window.fireAnimationId) {
            cancelAnimationFrame(window.fireAnimationId);
            window.fireAnimationId = null;
        }

        if (window.fireResizeHandler) {
            window.removeEventListener('resize', window.fireResizeHandler);
            window.fireResizeHandler = null;
        }

        window.fireFlames.length = 0;
        window.fireSparks.length = 0;
    };

    // Адаптация размера canvas
    window.fireResizeCanvas = function() {
        const canvas = document.getElementById('waveCanvas');
        if (!canvas) return;

        const rect = canvas.getBoundingClientRect();
        canvas.width = rect.width;
        canvas.height = Math.min(400, rect.width * 0.5);
    };

    // Класс для языков пламени
    window.Flame = class Flame {
        constructor(x, y) {
            this.x = x;
            this.y = y;
            this.baseY = y;
            this.life = 1.0;
            this.decay = 0.003 + Math.random() * 0.005;
            this.vx = (Math.random() - 0.5) * 2;
            this.vy = -2 - Math.random() * 3;
            this.size = 20 + Math.random() * 30;
            this.hue = 10 + Math.random() * 30; // Оранжевый-желтый
        }

        update() {
            this.x += this.vx + Math.sin(window.fireTime * 2 + this.y * 0.01) * 0.5;
            this.y += this.vy;
            this.vy *= 0.98; // Замедление
            this.life -= this.decay;
            this.size *= 0.98;
        }

        draw(ctx) {
            if (this.life <= 0) return false;

            const gradient = ctx.createRadialGradient(
                this.x, this.y, 0,
                this.x, this.y, this.size
            );

            // Центр - яркий желто-белый
            gradient.addColorStop(0, `hsla(${this.hue + 40}, 100%, ${50 + this.life * 50}%, ${this.life * 0.9})`);
            // Середина - оранжевый
            gradient.addColorStop(0.4, `hsla(${this.hue}, 100%, ${40 + this.life * 20}%, ${this.life * 0.7})`);
            // Край - красный
            gradient.addColorStop(0.7, `hsla(${this.hue - 20}, 100%, 40%, ${this.life * 0.4})`);
            // Внешний край - темно-красный, прозрачный
            gradient.addColorStop(1, `hsla(0, 80%, 20%, 0)`);

            ctx.fillStyle = gradient;
            ctx.beginPath();
            ctx.arc(this.x, this.y, this.size, 0, Math.PI * 2);
            ctx.fill();

            return true;
        }
    };

    // Класс для искр
    window.Spark = class Spark {
        constructor(x, y) {
            this.x = x;
            this.y = y;
            this.vx = (Math.random() - 0.5) * 4;
            this.vy = -5 - Math.random() * 5;
            this.life = 1.0;
            this.decay = 0.01 + Math.random() * 0.02;
            this.size = 2 + Math.random() * 3;
            this.gravity = 0.15;
        }

        update() {
            this.x += this.vx;
            this.y += this.vy;
            this.vy += this.gravity;
            this.life -= this.decay;
        }

        draw(ctx) {
            if (this.life <= 0) return false;

            ctx.save();
            ctx.globalAlpha = this.life;

            // Свечение искры
            const gradient = ctx.createRadialGradient(
                this.x, this.y, 0,
                this.x, this.y, this.size * 2
            );
            gradient.addColorStop(0, 'rgba(255, 220, 100, 1)');
            gradient.addColorStop(0.5, 'rgba(255, 150, 50, 0.5)');
            gradient.addColorStop(1, 'rgba(255, 100, 0, 0)');

            ctx.fillStyle = gradient;
            ctx.beginPath();
            ctx.arc(this.x, this.y, this.size * 2, 0, Math.PI * 2);
            ctx.fill();

            // Ядро искры
            ctx.fillStyle = 'rgba(255, 255, 200, 1)';
            ctx.beginPath();
            ctx.arc(this.x, this.y, this.size, 0, Math.PI * 2);
            ctx.fill();

            ctx.restore();
            return true;
        }
    };

    // Функция создания пламени
    window.fireCreateFlames = function(canvas) {
        // Создаем новые языки пламени снизу
        const flameCount = 3 + Math.floor(Math.random() * 3);
        for (let i = 0; i < flameCount; i++) {
            const x = canvas.width * 0.3 + Math.random() * canvas.width * 0.4;
            window.fireFlames.push(new window.Flame(x, canvas.height));
        }

        // Создаем искры
        if (Math.random() > 0.7) {
            const x = canvas.width * 0.35 + Math.random() * canvas.width * 0.3;
            window.fireSparks.push(new window.Spark(x, canvas.height - 10));
        }
    };

    // Функция анимации
    window.fireAnimate = function(canvas, ctx) {
        // Проверка существования элементов
        const waveCountElement = document.getElementById('waveCount');
        if (!waveCountElement || !document.contains(canvas)) {
            console.log('Элементы удалены из DOM, прекращаем анимацию');
            window.fireCleanup();
            return;
        }

        window.fireTime += 0.05;

        // Создаем фон с градиентом (темный внизу, еще темнее вверху)
        const bgGradient = ctx.createLinearGradient(0, canvas.height, 0, 0);
        bgGradient.addColorStop(0, 'rgba(10, 5, 5, 1)');
        bgGradient.addColorStop(0.5, 'rgba(5, 2, 2, 1)');
        bgGradient.addColorStop(1, 'rgba(0, 0, 0, 1)');
        ctx.fillStyle = bgGradient;
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        // Обновляем и рисуем языки пламени
        for (let i = window.fireFlames.length - 1; i >= 0; i--) {
            const flame = window.fireFlames[i];
            flame.update();
            if (!flame.draw(ctx)) {
                window.fireFlames.splice(i, 1);
            }
        }

        // Обновляем и рисуем искры
        for (let i = window.fireSparks.length - 1; i >= 0; i--) {
            const spark = window.fireSparks[i];
            spark.update();
            if (!spark.draw(ctx)) {
                window.fireSparks.splice(i, 1);
            }
        }

        // Создаем новые языки пламени
        window.fireCreateFlames(canvas);

        // Добавляем свечение внизу
        const glowGradient = ctx.createRadialGradient(
            canvas.width / 2, canvas.height, 0,
            canvas.width / 2, canvas.height, canvas.height * 0.6
        );
        glowGradient.addColorStop(0, 'rgba(255, 150, 50, 0.3)');
        glowGradient.addColorStop(0.5, 'rgba(255, 80, 20, 0.1)');
        glowGradient.addColorStop(1, 'rgba(100, 0, 0, 0)');
        ctx.fillStyle = glowGradient;
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        // Обновление счетчика
        waveCountElement.textContent = window.fireFlames.length + window.fireSparks.length;

        window.fireAnimationId = requestAnimationFrame(() => window.fireAnimate(canvas, ctx));
    };

    console.log('Библиотека огня готова к использованию!');
})();
