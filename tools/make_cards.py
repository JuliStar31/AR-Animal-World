"""Generates the printable animal cards used as Vuforia Image Targets.

Vuforia tracks images by their corners and contrast, so each card has a
busy, non-repeating background of outlined shapes around a big animal
drawing. Output: Assets/Cards/{singa,gajah,kelinci}.png (1200x1600 px,
print at 8 x 10.7 cm or larger).

    python3 tools/make_cards.py
"""
import math
import os
import random

from PIL import Image, ImageDraw, ImageFont

W, H = 1200, 1600
INK = (40, 44, 60)
OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Cards")
FONT = "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"


def font(size):
    try:
        return ImageFont.truetype(FONT, size)
    except OSError:
        return ImageFont.load_default()


def star(cx, cy, r, rot):
    pts = []
    for i in range(10):
        a = rot + i * math.pi / 5
        rr = r if i % 2 == 0 else r * 0.45
        pts.append((cx + math.cos(a) * rr, cy + math.sin(a) * rr))
    return pts


def background(d, rng, base, palette):
    d.rectangle([0, 0, W, H], fill=base)
    for _ in range(260):
        x, y = rng.uniform(0, W), rng.uniform(0, H)
        r = rng.uniform(14, 60)
        color = rng.choice(palette)
        kind = rng.random()
        if kind < 0.35:
            d.ellipse([x - r, y - r, x + r, y + r], fill=color, outline=INK, width=5)
        elif kind < 0.6:
            a = rng.uniform(0, math.tau)
            pts = [(x + math.cos(a + k * math.tau / 3) * r, y + math.sin(a + k * math.tau / 3) * r) for k in range(3)]
            d.polygon(pts, fill=color, outline=INK, width=5)
        elif kind < 0.8:
            d.polygon(star(x, y, r, rng.uniform(0, math.tau)), fill=color, outline=INK, width=4)
        else:
            w, h = r * rng.uniform(0.8, 1.8), r * rng.uniform(0.5, 1.2)
            d.rounded_rectangle([x - w, y - h, x + w, y + h], radius=8, fill=color, outline=INK, width=5)


def panel_details(d, rng, palette):
    # Small outlined shapes inside the white panel, kept clear of the
    # animal, so the middle of the card has trackable corners too.
    placed = 0
    while placed < 45:
        x, y = rng.uniform(175, W - 175), rng.uniform(215, 1085)
        if math.hypot(x - W / 2, (y - 690) * 0.8) < 360:
            continue
        r = rng.uniform(12, 30)
        color = rng.choice(palette[:3])
        if rng.random() < 0.5:
            d.polygon(star(x, y, r, rng.uniform(0, math.tau)), fill=color, outline=INK, width=4)
        else:
            d.ellipse([x - r, y - r, x + r, y + r], fill=color, outline=INK, width=4)
        placed += 1


def oval(d, box, fill, width=9):
    d.ellipse(box, fill=fill, outline=INK, width=width)


def eyes(d, cx, cy, gap, r=22):
    for sx in (-1, 1):
        x = cx + sx * gap
        d.ellipse([x - r, cy - r, x + r, cy + r], fill=INK)
        d.ellipse([x - r * 0.35 + 6, cy - r * 0.55, x + r * 0.35 + 6, cy + r * 0.15 - 4], fill="white")


def lion(d, cx, cy):
    fur, mane = (255, 196, 92), (214, 112, 40)
    for i in range(16):
        a = i * math.tau / 16
        x, y = cx + math.cos(a) * 190, cy + math.sin(a) * 190
        oval(d, [x - 85, y - 85, x + 85, y + 85], mane)
    oval(d, [cx - 230, cy - 230, cx + 230, cy + 230], mane)
    oval(d, [cx - 175, cy - 245, cx - 95, cy - 165], fur)
    oval(d, [cx + 95, cy - 245, cx + 175, cy - 165], fur)
    oval(d, [cx - 185, cy - 175, cx + 185, cy + 185], fur)
    eyes(d, cx, cy - 40, 70)
    oval(d, [cx - 110, cy + 20, cx + 110, cy + 150], (255, 226, 160))
    d.polygon([(cx - 35, cy + 30), (cx + 35, cy + 30), (cx, cy + 70)], fill=INK)
    d.arc([cx - 70, cy + 50, cx, cy + 120], 20, 160, fill=INK, width=8)
    d.arc([cx, cy + 50, cx + 70, cy + 120], 20, 160, fill=INK, width=8)
    for sx in (-1, 1):
        for k in range(3):
            y = cy + 75 + k * 22
            d.line([cx + sx * 115, y, cx + sx * 200, y - 10 + k * 10], fill=INK, width=5)


def elephant(d, cx, cy):
    skin, ear = (150, 168, 200), (205, 170, 210)
    oval(d, [cx - 330, cy - 200, cx - 90, cy + 140], ear)
    oval(d, [cx + 90, cy - 200, cx + 330, cy + 140], ear)
    oval(d, [cx - 270, cy - 150, cx - 140, cy + 80], (235, 200, 225), 6)
    oval(d, [cx + 140, cy - 150, cx + 270, cy + 80], (235, 200, 225), 6)
    oval(d, [cx - 170, cy - 210, cx + 170, cy + 130], skin)
    trunk = [(cx - 55, cy + 60), (cx + 55, cy + 60), (cx + 50, cy + 200), (cx + 95, cy + 260),
             (cx + 60, cy + 300), (cx + 5, cy + 230), (cx - 45, cy + 200)]
    d.polygon(trunk, fill=skin, outline=INK, width=9)
    for k in range(4):
        y = cy + 110 + k * 35
        d.line([cx - 40, y, cx + 40, y + 5], fill=INK, width=5)
    eyes(d, cx, cy - 50, 75)
    for sx in (-1, 1):
        d.polygon([(cx + sx * 70, cy + 70), (cx + sx * 120, cy + 140), (cx + sx * 95, cy + 75)],
                  fill="white", outline=INK, width=6)


def rabbit(d, cx, cy):
    fur, inner = (250, 247, 240), (255, 182, 196)
    for sx in (-1, 1):
        x = cx + sx * 85
        oval(d, [x - 60, cy - 420, x + 60, cy - 110], fur)
        oval(d, [x - 30, cy - 380, x + 30, cy - 150], inner, 6)
    oval(d, [cx - 200, cy - 170, cx + 200, cy + 200], fur)
    eyes(d, cx, cy - 20, 75, 26)
    oval(d, [cx - 25, cy + 50, cx + 25, cy + 90], inner, 6)
    d.arc([cx - 50, cy + 70, cx, cy + 130], 20, 160, fill=INK, width=7)
    d.arc([cx, cy + 70, cx + 50, cy + 130], 20, 160, fill=INK, width=7)
    d.rectangle([cx - 28, cy + 120, cx + 28, cy + 165], fill="white", outline=INK, width=6)
    d.line([cx, cy + 120, cx, cy + 165], fill=INK, width=5)
    for sx in (-1, 1):
        oval(d, [cx + sx * 150 - 35, cy + 50, cx + sx * 150 + 35, cy + 100], (255, 205, 210), 0)
    carrot = [(cx + 230, cy + 120), (cx + 290, cy + 140), (cx + 190, cy + 330)]
    d.polygon(carrot, fill=(255, 140, 60), outline=INK, width=7)
    for k in range(3):
        d.line([cx + 255 + k * 12, cy + 125, cx + 250 + k * 25, cy + 60], fill=(60, 160, 80), width=12)


CARDS = [
    ("singa", "SINGA", lion, (255, 236, 179), [(255, 214, 91), (255, 140, 90), (116, 201, 110), (255, 255, 255), (120, 180, 240)], 11),
    ("gajah", "GAJAH", elephant, (213, 236, 255), [(58, 125, 202), (140, 195, 242), (255, 217, 91), (255, 255, 255), (190, 150, 220)], 22),
    ("kelinci", "KELINCI", rabbit, (220, 245, 214), [(116, 201, 110), (45, 156, 98), (255, 179, 108), (255, 255, 255), (255, 160, 190)], 33),
]


def make(name, label, draw_animal, base, palette, seed):
    rng = random.Random(seed)
    img = Image.new("RGB", (W, H))
    d = ImageDraw.Draw(img)
    background(d, rng, base, palette)

    d.rounded_rectangle([130, 170, W - 130, 1130], radius=70, fill="white", outline=INK, width=12)
    panel_details(d, rng, palette)
    draw_animal(d, W // 2, 690)

    d.rounded_rectangle([170, 1210, W - 170, 1430], radius=60, fill=palette[0], outline=INK, width=12)
    f = font(150)
    tw = d.textlength(label, font=f)
    d.text(((W - tw) / 2, 1235), label, font=f, fill=INK)

    d.rectangle([0, 0, W - 1, H - 1], outline=INK, width=24)
    os.makedirs(OUT, exist_ok=True)
    path = os.path.join(OUT, name + ".png")
    img.save(path, optimize=True)
    return path


if __name__ == "__main__":
    for card in CARDS:
        print(make(*card))
