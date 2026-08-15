/**
 * Marwadi Ghee Sweets — Cart Module
 * Handles all LocalStorage-based cart operations
 */
const CART_KEY = 'mgs_cart';

const Cart = {
    get() {
        try { return JSON.parse(localStorage.getItem(CART_KEY) || '[]'); }
        catch { return []; }
    },

    save(items) {
        localStorage.setItem(CART_KEY, JSON.stringify(items));
        Cart.updateBadges();
        Cart.dispatchChange();
    },

    add(productId, productSlug, productName, imageUrl, weightLabel, price) {
        const items = Cart.get();
        const key   = `${productId}__${weightLabel}`;
        const idx   = items.findIndex(i => i._key === key);
        if (idx >= 0) {
            items[idx].quantity += 1;
        } else {
            items.push({ _key: key, productId, productSlug, productName, imageUrl, weightLabel, price: parseFloat(price), quantity: 1 });
        }
        Cart.save(items);
        Cart.showToast(`${productName} (${weightLabel}) added to cart!`);
    },

    remove(key) {
        const items = Cart.get().filter(i => i._key !== key);
        Cart.save(items);
    },

    updateQty(key, delta) {
        const items = Cart.get();
        const idx   = items.findIndex(i => i._key === key);
        if (idx < 0) return;
        items[idx].quantity = Math.max(1, items[idx].quantity + delta);
        Cart.save(items);
    },

    setQty(key, qty) {
        const items = Cart.get();
        const idx   = items.findIndex(i => i._key === key);
        if (idx < 0) return;
        const n = parseInt(qty);
        if (isNaN(n) || n < 1) return;
        items[idx].quantity = n;
        Cart.save(items);
    },

    clear() {
        localStorage.removeItem(CART_KEY);
        Cart.updateBadges();
        Cart.dispatchChange();
    },

    count() {
        return Cart.get().reduce((s, i) => s + i.quantity, 0);
    },

    subtotal() {
        return Cart.get().reduce((s, i) => s + i.price * i.quantity, 0);
    },

    deliveryCharge(freeThreshold, chargeAmount) {
        const sub = Cart.subtotal();
        return sub >= freeThreshold ? 0 : chargeAmount;
    },

    grandTotal(freeThreshold, chargeAmount) {
        return Cart.subtotal() + Cart.deliveryCharge(freeThreshold, chargeAmount);
    },

    updateBadges() {
        const count = Cart.count();
        document.querySelectorAll('.cart-count-badge, .js-cart-count').forEach(el => {
            el.textContent = count;
            el.style.display = count > 0 ? 'flex' : 'none';
        });
    },

    dispatchChange() {
        window.dispatchEvent(new CustomEvent('cart:changed', { detail: { items: Cart.get() } }));
    },

    showToast(msg) {
        let t = document.getElementById('cart-toast');
        if (!t) {
            t = document.createElement('div');
            t.id = 'cart-toast';
            t.style.cssText = 'position:fixed;bottom:100px;left:50%;transform:translateX(-50%);' +
                'background:#800020;color:#fff;padding:12px 24px;border-radius:30px;font-size:14px;' +
                'font-weight:600;z-index:9999;white-space:nowrap;box-shadow:0 4px 16px rgba(0,0,0,.2);' +
                'transition:opacity .3s;pointer-events:none;';
            document.body.appendChild(t);
        }
        t.textContent = '🛒 ' + msg;
        t.style.opacity = '1';
        clearTimeout(t._timer);
        t._timer = setTimeout(() => { t.style.opacity = '0'; }, 2500);
    }
};

// ── Cart Drawer ────────────────────────────────────────────────────────────
const CartDrawer = {
    open() {
        CartDrawer.render();
        document.getElementById('cart-drawer')?.classList.add('open');
        document.getElementById('cart-drawer-overlay')?.classList.add('open');
        document.body.style.overflow = 'hidden';
    },
    close() {
        document.getElementById('cart-drawer')?.classList.remove('open');
        document.getElementById('cart-drawer-overlay')?.classList.remove('open');
        document.body.style.overflow = '';
    },

    render() {
        const body = document.getElementById('cart-drawer-body');
        const footer = document.getElementById('cart-drawer-footer');
        if (!body) return;

        const items = Cart.get();
        const freeThreshold  = parseFloat(document.body.dataset.freeThreshold  || '500');
        const deliveryCharge = parseFloat(document.body.dataset.deliveryCharge || '50');

        if (items.length === 0) {
            body.innerHTML = `<div class="text-center py-5">
                <div style="font-size:48px;margin-bottom:16px;">🛒</div>
                <p style="color:var(--text-muted)">Your cart is empty</p>
                <a href="/sweets" class="btn-primary-brand" style="font-size:13px;padding:10px 20px;margin-top:8px;">
                    Browse Sweets
                </a>
            </div>`;
            if (footer) footer.innerHTML = '';
            return;
        }

        body.innerHTML = items.map(item => `
            <div class="cart-drawer-item" data-key="${item._key}">
                <img src="${item.imageUrl || '/images/placeholder.jpg'}" alt="${item.productName}" class="cart-drawer-img" onerror="this.src='/images/placeholder.jpg'">
                <div style="flex:1;min-width:0;">
                    <div style="font-weight:700;font-size:14px;color:var(--text-dark);margin-bottom:2px;">${item.productName}</div>
                    <div style="font-size:12px;color:var(--gold);margin-bottom:8px;">${item.weightLabel}</div>
                    <div style="display:flex;align-items:center;justify-content:space-between;">
                        <div class="qty-control" style="border-width:1px;">
                            <button class="qty-btn" style="width:32px;height:34px;font-size:16px;" onclick="Cart.updateQty('${item._key}',-1);CartDrawer.render()">−</button>
                            <span style="width:36px;text-align:center;font-size:14px;font-weight:700;">${item.quantity}</span>
                            <button class="qty-btn" style="width:32px;height:34px;font-size:16px;" onclick="Cart.updateQty('${item._key}',1);CartDrawer.render()">+</button>
                        </div>
                        <span style="font-family:var(--font-serif);font-weight:700;color:var(--burgundy);">₹${(item.price * item.quantity).toFixed(0)}</span>
                    </div>
                </div>
                <button onclick="Cart.remove('${item._key}');CartDrawer.render()" 
                    style="background:none;border:none;color:var(--text-muted);font-size:18px;cursor:pointer;padding:4px;align-self:flex-start;">×</button>
            </div>`).join('');

        const sub      = Cart.subtotal();
        const delivery = Cart.deliveryCharge(freeThreshold, deliveryCharge);
        const total    = sub + delivery;
        const remaining = freeThreshold - sub;

        if (footer) footer.innerHTML = `
            ${remaining > 0
                ? `<div class="free-shipping-bar mb-3">
                    <div>Add <strong>₹${remaining.toFixed(0)}</strong> more for FREE delivery</div>
                    <div class="free-shipping-track"><div class="free-shipping-progress" style="width:${Math.min(100,(sub/freeThreshold)*100).toFixed(0)}%"></div></div>
                   </div>` : `<div class="free-shipping-bar mb-3" style="text-align:center;">🎉 You've unlocked <strong>FREE delivery!</strong></div>`}
            <div style="display:flex;justify-content:space-between;font-size:14px;margin-bottom:6px;">
                <span>Subtotal</span><span>₹${sub.toFixed(0)}</span>
            </div>
            <div style="display:flex;justify-content:space-between;font-size:14px;margin-bottom:12px;">
                <span>Delivery</span><span>${delivery > 0 ? '₹'+delivery.toFixed(0) : '<strong style="color:green">FREE</strong>'}</span>
            </div>
            <div style="display:flex;justify-content:space-between;font-family:var(--font-serif);font-size:18px;font-weight:700;margin-bottom:16px;padding-top:12px;border-top:1px solid var(--border);">
                <span>Total</span><span style="color:var(--burgundy)">₹${total.toFixed(0)}</span>
            </div>
            <a href="/Checkout" class="btn-primary-brand" style="width:100%;justify-content:center;" onclick="CartDrawer.close()">
                Checkout via WhatsApp 📲
            </a>
            <button onclick="CartDrawer.close()" style="width:100%;background:none;border:1px solid var(--border);border-radius:30px;padding:10px;font-size:13px;margin-top:8px;cursor:pointer;color:var(--text-muted);">
                Continue Shopping
            </button>`;
    }
};

// ── Site-wide Init ─────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    Cart.updateBadges();

    // Sticky header shadow
    const header = document.querySelector('.site-header');
    if (header) {
        const onScroll = () => header.classList.toggle('scrolled', window.scrollY > 10);
        window.addEventListener('scroll', onScroll, { passive: true });
    }

    // Cart drawer toggle
    document.querySelectorAll('.js-open-cart').forEach(el =>
        el.addEventListener('click', e => { e.preventDefault(); CartDrawer.open(); }));
    document.getElementById('cart-drawer-overlay')?.addEventListener('click', CartDrawer.close);
    document.getElementById('cart-drawer-close')?.addEventListener('click', CartDrawer.close);

    // Mobile menu
    const mobileMenuBtn  = document.getElementById('mobile-menu-btn');
    const mobileMenuPanel = document.getElementById('mobile-menu-panel');
    if (mobileMenuBtn && mobileMenuPanel) {
        mobileMenuBtn.addEventListener('click', () => {
            const open = mobileMenuPanel.classList.toggle('d-block');
            mobileMenuPanel.classList.toggle('d-none', !open);
        });
    }

    // Lazy load images
    if ('IntersectionObserver' in window) {
        const imgObs = new IntersectionObserver((entries) => {
            entries.forEach(e => {
                if (e.isIntersecting) {
                    const img = e.target;
                    if (img.dataset.src) { img.src = img.dataset.src; img.removeAttribute('data-src'); }
                    imgObs.unobserve(img);
                }
            });
        }, { rootMargin: '200px' });
        document.querySelectorAll('img[data-src]').forEach(img => imgObs.observe(img));
    }

    // Scroll animations
    if ('IntersectionObserver' in window) {
        const obs = new IntersectionObserver((entries) => {
            entries.forEach(e => {
                if (e.isIntersecting) {
                    e.target.classList.add('page-fade-in');
                    obs.unobserve(e.target);
                }
            });
        }, { threshold: 0.1 });
        document.querySelectorAll('.animate-on-scroll').forEach(el => obs.observe(el));
    }
});

// ── Card weight selector helpers ───────────────────────────────────────────
/**
 * Called when a weight pill on a product card is clicked.
 * Updates the active pill, price display, and the Add-to-Cart button state.
 */
function cardSelectWeight(btn) {
    const cardId = btn.dataset.card;

    // Deactivate siblings, activate clicked
    document.querySelectorAll(`.card-weight-btn[data-card="${cardId}"]`)
        .forEach(b => b.classList.remove('active'));
    btn.classList.add('active');

    // Update displayed price
    const price    = parseFloat(btn.dataset.price);
    const original = btn.dataset.original;
    const priceEl  = document.getElementById('price-' + cardId);
    const origEl   = document.getElementById('orig-'  + cardId);
    if (priceEl) priceEl.textContent = '₹' + price.toFixed(0);
    if (origEl) {
        if (original) {
            origEl.textContent    = '₹' + original;
            origEl.style.display  = 'inline';
        } else {
            origEl.style.display  = 'none';
        }
    }

    // Re-wire the Add-to-Cart button with the newly selected weight
    const atcBtn = document.getElementById('atc-' + cardId);
    if (atcBtn) {
        atcBtn.onclick = () => cardAddToCart(
            cardId,
            btn.dataset.id,
            btn.dataset.slug,
            btn.dataset.name,
            btn.dataset.image,
            btn.dataset.label,
            price
        );
    }
}

/**
 * Adds the selected weight/price for a card to the cart.
 * Used by both the initial onclick (set server-side) and after weight change.
 */
function cardAddToCart(cardId, productId, productSlug, productName, imageUrl, weightLabel, price) {
    Cart.add(productId, productSlug, productName, imageUrl, weightLabel, price);
}

// Expose globally
window.Cart = Cart;
window.CartDrawer = CartDrawer;
window.cardSelectWeight = cardSelectWeight;
window.cardAddToCart    = cardAddToCart;
