"""Draws the flat illustrations and icons used by the game UI.

The style follows the UI mockups: soft flat shapes, no outlines on the
animals, rounded strokes on icons. Icons are white so Unity can tint them;
illustrations keep their colours. Everything is drawn at 4x and scaled
down for smooth edges.

    python3 tools/make_ui_art.py   ->  Assets/Art/UI/*.png
"""
import math
import os

from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Art", "UI")
SS = 4  # supersampling

BROWN = (59, 42, 20)
WHITE = (255, 255, 255)


class Canvas:
    """Drawing in a 1000x1000 design space, rendered at size*SS."""

    def __init__(self, size, design=1000):
        self.size = size
        self.k = size * SS / design
        self.img = Image.new("RGBA", (size * SS, size * SS), (0, 0, 0, 0))
        self.d = ImageDraw.Draw(self.img)

    def p(self, *v):
        return [x * self.k for x in v]

    def circle(self, cx, cy, r, fill):
        self.d.ellipse(self.p(cx - r, cy - r, cx + r, cy + r), fill=fill)

    def ellipse(self, cx, cy, rx, ry, fill, outline=None, width=0):
        self.d.ellipse(self.p(cx - rx, cy - ry, cx + rx, cy + ry), fill=fill,
                       outline=outline, width=int(width * self.k) if width else 0)

    def poly(self, pts, fill):
        self.d.polygon([(x * self.k, y * self.k) for x, y in pts], fill=fill)

    def stroke(self, pts, color, width):
        w = int(width * self.k)
        scaled = [(x * self.k, y * self.k) for x, y in pts]
        self.d.line(scaled, fill=color, width=w, joint="curve")
        for x, y in (scaled[0], scaled[-1]):
            self.d.ellipse([x - w / 2, y - w / 2, x + w / 2, y + w / 2], fill=color)

    def arc(self, cx, cy, r, start, end, color, width, steps=40):
        pts = [(cx + math.cos(math.radians(a)) * r, cy + math.sin(math.radians(a)) * r)
               for a in [start + (end - start) * i / steps for i in range(steps + 1)]]
        self.stroke(pts, color, width)

    def rrect(self, x0, y0, x1, y1, r, fill, outline=None, width=0):
        self.d.rounded_rectangle(self.p(x0, y0, x1, y1), radius=r * self.k, fill=fill,
                                 outline=outline, width=int(width * self.k) if width else 0)

    def save(self, name):
        os.makedirs(OUT, exist_ok=True)
        out = self.img.resize((self.size, self.size), Image.LANCZOS)
        out.save(os.path.join(OUT, name + ".png"), optimize=True)


def eye(c, x, y, rx=38, ry=46):
    c.ellipse(x, y, rx, ry, BROWN)
    c.circle(x + rx * 0.3, y - ry * 0.4, rx * 0.36, WHITE)


def lion(happy=False):
    c = Canvas(512)
    mane, face, muzzle = (224, 128, 42), (255, 193, 94), (255, 241, 217)
    c.circle(500, 500, 330, mane)
    for i in range(8):
        a = math.radians(-90 + i * 45)
        c.circle(500 + math.cos(a) * 300, 500 + math.sin(a) * 300, 165, mane)
    for x in (330, 670):
        c.circle(x, 300, 78, face)
        c.circle(x, 300, 46, (247, 163, 163))
    c.circle(500, 520, 290, face)
    c.circle(335, 625, 44, (255, 169, 138))
    c.circle(665, 625, 44, (255, 169, 138))
    c.ellipse(500, 655, 140, 95, muzzle)
    if happy:
        c.arc(400, 545, 48, 200, 340, BROWN, 20)
        c.arc(600, 545, 48, 200, 340, BROWN, 20)
        c.poly([(455, 590), (545, 590), (500, 628)], (192, 80, 58))
        c.d.chord(c.p(415, 600, 585, 750), 0, 180, fill=(181, 64, 46), outline=BROWN, width=int(14 * c.k))
    else:
        eye(c, 400, 520)
        eye(c, 600, 520)
        c.poly([(455, 595), (545, 595), (500, 640)], (192, 80, 58))
        c.stroke([(500, 640), (500, 672)], BROWN, 16)
        c.arc(455, 655, 46, 20, 160, BROWN, 16)
        c.arc(545, 655, 46, 20, 160, BROWN, 16)
    c.save("lion_happy" if happy else "lion")


def elephant():
    c = Canvas(512)
    skin, ear, inner = (169, 203, 234), (141, 182, 224), (245, 181, 197)
    for sx in (-1, 1):
        c.circle(500 + sx * 280, 460, 205, ear)
        c.circle(500 + sx * 300, 465, 130, inner)
    c.circle(500, 430, 265, skin)
    # Trunk: circles along a curve, curling up at the tip.
    pts = []
    for i in range(60):
        t = i / 59
        x = (1 - t) ** 3 * 500 + 3 * (1 - t) ** 2 * t * 505 + 3 * (1 - t) * t ** 2 * 470 + t ** 3 * 360
        y = (1 - t) ** 3 * 600 + 3 * (1 - t) ** 2 * t * 820 + 3 * (1 - t) * t ** 2 * 900 + t ** 3 * 820
        pts.append((x, y, 95 - 25 * t))
    for x, y, r in pts:
        c.circle(x, y, r, skin)
    c.circle(352, 800, 72, skin)
    c.arc(470, 700, 50, 200, 340, ear, 12)
    c.arc(470, 770, 46, 200, 340, ear, 12)
    eye(c, 420, 420, 30, 36)
    eye(c, 580, 420, 30, 36)
    c.circle(355, 520, 40, (245, 181, 197))
    c.circle(645, 520, 40, (245, 181, 197))
    c.save("elephant")


def rabbit():
    c = Canvas(512)
    edge = (234, 222, 205)
    for x in (395, 605):
        c.ellipse(x, 270, 72, 225, WHITE, edge, 10)
        c.ellipse(x, 280, 36, 175, (247, 163, 191))
    c.ellipse(500, 640, 265, 255, WHITE, edge, 12)
    eye(c, 405, 610, 28, 36)
    eye(c, 595, 610, 28, 36)
    c.circle(345, 700, 40, (255, 196, 205))
    c.circle(655, 700, 40, (255, 196, 205))
    c.ellipse(500, 680, 28, 19, (244, 143, 177))
    c.arc(475, 705, 26, 20, 160, BROWN, 9)
    c.arc(525, 705, 26, 20, 160, BROWN, 9)
    c.rrect(478, 728, 522, 775, 8, WHITE, BROWN, 6)
    c.save("rabbit")


def star_points(cx, cy, r_out, r_in, rot=-90):
    pts = []
    for i in range(10):
        a = math.radians(rot + i * 36)
        r = r_out if i % 2 == 0 else r_in
        pts.append((cx + math.cos(a) * r, cy + math.sin(a) * r))
    return pts


def star(name, fill, line):
    c = Canvas(256)
    pts = star_points(500, 530, 440, 190)
    c.poly(pts, fill)
    c.stroke(pts + [pts[0]], line, 55)
    c.save(name)


def icon(name, draw, size=128):
    c = Canvas(size)
    draw(c)
    c.save(name)


def camera(c, color=WHITE, width=70):
    c.rrect(130, 300, 870, 800, 120, None, color, width)
    c.rrect(360, 190, 640, 320, 50, color)
    c.ellipse(500, 550, 145, 145, None, color, width)


def camera_off():
    c = Canvas(256)
    yellow = (255, 201, 60)
    c.rrect(380, 200, 620, 320, 40, None, BROWN, 55)
    c.rrect(130, 290, 870, 800, 130, yellow, BROWN, 55)
    c.ellipse(500, 545, 140, 140, WHITE, BROWN, 55)
    c.stroke([(220, 860), (800, 200)], BROWN, 60)
    c.save("camera_off")


def speaker(c):
    c.poly([(130, 380), (290, 380), (520, 190), (520, 810), (290, 620), (130, 620)], WHITE)
    c.arc(560, 500, 150, -50, 50, WHITE, 70)
    c.arc(560, 500, 290, -50, 50, WHITE, 70)


def cloud(c):
    for x, y, r in ((300, 600, 200), (520, 470, 260), (760, 620, 180)):
        c.circle(x, y, r, WHITE)
    c.rrect(100, 600, 940, 800, 100, WHITE)


def corner(c):
    # Top-left corner bracket of the scan frame.
    w = 90
    c.stroke([(w / 2, 1000), (w / 2, 330)], WHITE, w)
    c.arc(330, 330, 330 - w / 2, 180, 270, WHITE, w)
    c.stroke([(330, w / 2), (1000, w / 2)], WHITE, w)


if __name__ == "__main__":
    lion()
    lion(happy=True)
    elephant()
    rabbit()
    star("star_on", (255, 201, 60), (217, 162, 26))
    star("star_off", (230, 237, 243), (195, 207, 218))
    camera_off()
    icon("chevron_left", lambda c: c.stroke([(620, 200), (330, 500), (620, 800)], WHITE, 110))
    icon("chevron_right", lambda c: c.stroke([(380, 200), (670, 500), (380, 800)], WHITE, 110))
    icon("play", lambda c: c.poly([(250, 150), (850, 500), (250, 850)], WHITE))
    icon("star_outline", lambda c: c.stroke(star_points(500, 540, 420, 180) + [star_points(500, 540, 420, 180)[0]], WHITE, 75))
    icon("camera", camera)
    icon("speaker", speaker)
    icon("check", lambda c: c.stroke([(170, 520), (400, 750), (840, 270)], WHITE, 120))
    icon("arrow_up", lambda c: (c.stroke([(500, 870), (500, 170)], WHITE, 110), c.stroke([(220, 440), (500, 160), (780, 440)], WHITE, 110)))
    icon("cloud", cloud, 256)
    icon("scan_corner", corner, 256)
    print(sorted(os.listdir(OUT)))
