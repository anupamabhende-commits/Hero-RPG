from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER
from reportlab.lib.pagesizes import letter
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.units import inch
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, PageBreak, KeepTogether

OUT = r"E:\Hero-RPG\Player_Code_Explained.pdf"
NAVY = colors.HexColor("#17365D")
BLUE = colors.HexColor("#2E74B5")
SLATE = colors.HexColor("#4B5563")
PALE = colors.HexColor("#EAF2F8")
TABLE_HEAD = colors.HexColor("#E8EEF5")

styles = getSampleStyleSheet()
styles.add(ParagraphStyle(name="GuideTitle", parent=styles["Title"], fontName="Helvetica-Bold", fontSize=25, leading=30, textColor=NAVY, spaceAfter=4))
styles.add(ParagraphStyle(name="Subtitle", parent=styles["BodyText"], fontName="Helvetica", fontSize=12, leading=16, textColor=SLATE, spaceAfter=14))
styles.add(ParagraphStyle(name="H1Guide", parent=styles["Heading1"], fontName="Helvetica-Bold", fontSize=16, leading=20, textColor=BLUE, spaceBefore=15, spaceAfter=6, keepWithNext=True))
styles.add(ParagraphStyle(name="H2Guide", parent=styles["Heading2"], fontName="Helvetica-Bold", fontSize=13, leading=16, textColor=NAVY, spaceBefore=9, spaceAfter=4, keepWithNext=True))
styles.add(ParagraphStyle(name="BodyGuide", parent=styles["BodyText"], fontName="Helvetica", fontSize=10.5, leading=14, spaceAfter=6))
styles.add(ParagraphStyle(name="BulletGuide", parent=styles["BodyText"], fontName="Helvetica", fontSize=10.5, leading=14, leftIndent=16, firstLineIndent=-8, bulletIndent=8, spaceAfter=3))
styles.add(ParagraphStyle(name="CodeGuide", parent=styles["BodyText"], fontName="Courier", fontSize=8.8, leading=11, textColor=colors.HexColor("#1F2937")))
styles.add(ParagraphStyle(name="NoteGuide", parent=styles["BodyText"], fontName="Helvetica-Bold", fontSize=10.3, leading=14, textColor=colors.HexColor("#7A5A00")))
styles.add(ParagraphStyle(name="SmallCell", parent=styles["BodyText"], fontName="Helvetica", fontSize=9.3, leading=12, spaceAfter=0))
styles.add(ParagraphStyle(name="SmallCellBold", parent=styles["BodyText"], fontName="Helvetica-Bold", fontSize=9.3, leading=12, textColor=NAVY, spaceAfter=0))

def P(text, style="BodyGuide"):
    return Paragraph(text, styles[style])

def bullets(items):
    return [Paragraph(item, styles["BulletGuide"], bulletText="•") for item in items]

def code(text):
    t = Table([[P(text, "CodeGuide")]], colWidths=[6.7 * inch])
    t.setStyle(TableStyle([("BACKGROUND", (0,0), (-1,-1), colors.HexColor("#F2F4F7")), ("BOX", (0,0), (-1,-1), .4, colors.HexColor("#D6DBE2")), ("LEFTPADDING", (0,0), (-1,-1), 9), ("RIGHTPADDING", (0,0), (-1,-1), 9), ("TOPPADDING", (0,0), (-1,-1), 7), ("BOTTOMPADDING", (0,0), (-1,-1), 7)]))
    return t

def data_table(headers, rows, widths):
    data = [[P(h, "SmallCellBold") for h in headers]]
    for row in rows:
        data.append([P(v, "SmallCellBold" if i == 0 else "SmallCell") for i, v in enumerate(row)])
    t = Table(data, colWidths=[w * inch for w in widths], repeatRows=1, hAlign="LEFT")
    commands = [("BACKGROUND", (0,0), (-1,0), TABLE_HEAD), ("GRID", (0,0), (-1,-1), .35, colors.HexColor("#CBD5E1")), ("VALIGN", (0,0), (-1,-1), "MIDDLE"), ("LEFTPADDING", (0,0), (-1,-1), 7), ("RIGHTPADDING", (0,0), (-1,-1), 7), ("TOPPADDING", (0,0), (-1,-1), 6), ("BOTTOMPADDING", (0,0), (-1,-1), 6)]
    t.setStyle(TableStyle(commands))
    return t

def footer(canvas, doc):
    canvas.saveState()
    canvas.setFont("Helvetica-Bold", 7.5)
    canvas.setFillColor(colors.HexColor("#6B7280"))
    canvas.drawRightString(letter[0] - 0.9*inch, letter[1] - 0.38*inch, "HERO RPG | PLAYER CODE GUIDE")
    canvas.setFont("Helvetica", 7.5)
    canvas.drawCentredString(letter[0]/2, 0.36*inch, f"Page {doc.page}")
    canvas.restoreState()

doc = SimpleDocTemplate(OUT, pagesize=letter, leftMargin=.9*inch, rightMargin=.9*inch, topMargin=.72*inch, bottomMargin=.62*inch)
story = []
story += [P("Player Code Explained", "GuideTitle"), P("A simple guide to how the player moves, fights, casts spells, and uses portals in Hero RPG.", "Subtitle")]
callout = Table([[P("<b>Big idea:</b> the Player object is not controlled by one huge script. It is a team of small scripts, where each script has one clear job.", "BodyGuide")]], colWidths=[6.7*inch])
callout.setStyle(TableStyle([("BACKGROUND", (0,0), (-1,-1), PALE), ("BOX", (0,0), (-1,-1), .5, colors.HexColor("#B8CCE4")), ("LEFTPADDING", (0,0), (-1,-1), 10), ("RIGHTPADDING", (0,0), (-1,-1), 10), ("TOPPADDING", (0,0), (-1,-1), 9), ("BOTTOMPADDING", (0,0), (-1,-1), 6)]))
story += [callout, Spacer(1, 8), P("1. The player is made of small systems", "H1Guide"), P("The Player GameObject contains several components. They share information through PlayerController, which acts like the coordinator.")]
story.append(data_table(["Script", "Simple job"], [
    ("PlayerInput", "Reads the keyboard and mouse. It remembers what the player pressed this frame."),
    ("PlayerController", "The main coordinator. It asks the other systems to move, rotate, jump, fight, or cast."),
    ("PlayerMovement", "Moves the CharacterController using the current input and movement settings."),
    ("PlayerRotation", "Turns the character toward the desired direction, including lock-on behavior."),
    ("PlayerJump + GroundChecker", "Checks whether the player is on the ground, then applies jumping and gravity."),
    ("PlayerCombat", "Receives attack, magic, and portal requests and starts the right action."),
    ("PlayerAnimator", "Sends Speed, Grounded, Sprint, and trigger values to the Animator."),
    ("PlayerSpellController", "Creates the fireball and runtime portal. It also manages casting cooldown time."),
    ("ActionRPGCamera", "Follows the player and handles camera angle and collision behavior."),
], [2.05, 4.65]))
story += [P("2. What happens every frame", "H1Guide"), P("Unity calls Update() once every frame. In this project, the basic order of events is:")]
story += bullets(["PlayerInput reads keys such as WASD, Space, Q, F, Tab, mouse buttons, and Shift.", "PlayerController reads that input and tells movement, rotation, combat, and animation systems what to do.", "PlayerMovement changes the CharacterController position.", "PlayerAnimator updates animation parameters so the model looks like it is walking, jumping, rolling, or casting."])
story += [code("Keyboard/mouse  ->  PlayerInput  ->  PlayerController  ->  specialist script  ->  CharacterController / Animator"), P("3. Movement, jumping, and camera", "H1Guide"), P("Movement", "H2Guide"), P("PlayerInput stores a movement direction. PlayerMovement uses the walk speed, sprint speed, acceleration, and deceleration from PlayerStats. The CharacterController is then moved safely, so it can collide with the ground and walls."), P("Jumping", "H2Guide"), P("GroundChecker checks whether the CharacterController is standing on ground. When the jump key is pressed and the player is grounded, PlayerJump adds upward movement. Gravity then pulls the player back down."), P("Camera and facing direction", "H2Guide"), P("ActionRPGCamera follows a camera target near the player. PlayerRotation uses the camera direction so pressing forward means move where the camera is looking, not simply move along the world's Z axis.")]
story += [P("4. Combat and spells", "H1Guide"), P("PlayerCombat is the bridge between normal player controls and the magic system. It checks whether the player is busy, then asks PlayerSpellController to begin a spell.")]
story.append(data_table(["Input", "What the code does"], [("Left mouse", "PlayerCombat starts the normal attack animation."), ("Right mouse", "PlayerCombat starts the magic or cast action."), ("1", "PlayerSpellController starts the fireball spell directly."), ("F", "PlayerCombat asks PlayerSpellController to cast a portal. The spell script also currently listens for F itself.")], [1.35, 5.35]))
story += [P("5. How a fireball works", "H1Guide"), P("The fireball is connected to the casting animation. Animation events can call methods in PlayerSpellController at the correct moment.")]
story += bullets(["CastFireball() checks whether another spell is already active.", "The Animator receives the Fireball trigger, starting the cast animation.", "SpawnFireball() creates a copy of the fireball prefab at the fire point.", "EnableFireballVFX() can show the charging effect while the spell is being prepared.", "DisableFireballVFX() launches it. The Fireball script moves it forward until it reaches maxDistance, then destroys it."])
story += [code("PlayerSpellController  ->  Animator event  ->  SpawnFireball  ->  Fireball.Launch(direction)  ->  move  ->  destroy")]
story += [P("6. How the portal works", "H1Guide"), P("Pressing F starts CastPortal(). After a short delay, the code creates a new GameObject called Runtime Portal in front of the player. PortalTeleportController builds the orange ring, blue interior, and sparks at runtime."), P("When the player crosses the portal opening, TeleportPlayer() moves the Player transform to the Portal Destination object in the scene. It temporarily disables the CharacterController first, which avoids collision problems while changing position."), P("Important portal settings", "H2Guide")]
story += bullets(["portalSpawnDelay: how long the game waits before the portal appears.", "portalForwardOffset: how far in front of the player the portal is created.", "portalRadius: the size of the portal's usable opening.", "portalLifetime: how long it stays open.", "portalDestination: the Transform where the player arrives."])
story += [P("Note: the current portal check measures the player's horizontal and vertical distance from the portal center. Since the portal is raised above ground, use only horizontal distance for the entry test or lower the portal's vertical offset. Otherwise walking through the visual opening may not teleport the player.", "NoteGuide")]
story += [P("7. Easy way to explain the code in a demo", "H1Guide")]
story += bullets(["Input reads what I press.", "The controller decides which player system should react.", "Movement, jumping, combat, animation, and spells are separate so each part is easier to change.", "The spell controller creates the fireball or portal, and the portal controller handles the actual teleport.", "PlayerStats keeps values such as speed and gravity in one place, so tuning the feel of the game is simple."])
story += [P("8. Files to open when explaining", "H1Guide")]
story += bullets(["Assets/Player/Scripts/PlayerInput.cs - where keyboard and mouse actions are read.", "Assets/Player/Scripts/PlayerController.cs - the player coordinator.", "Assets/Player/Scripts/PlayerMovement.cs and PlayerRotation.cs - moving and facing.", "Assets/Player/Scripts/PlayerCombat.cs - combat decisions.", "Assets/Scripts/PlayerSpellController.cs - fireball and portal casting.", "Assets/Scripts/PortalTeleportController.cs - portal visuals, detection, and teleporting."])
doc.build(story, onFirstPage=footer, onLaterPages=footer)
print(OUT)
