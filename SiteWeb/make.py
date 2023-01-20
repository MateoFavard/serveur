import os, sys, shutil


def main(argv):
    if len(argv) != 2:
        print("Too many or not enough arguments")
        return

    target = argv[1]

    if target == "all":
        make_static()
        make_style()
        make_elm()

    elif target == "static":
        make_static()

    elif target == "style":
        make_style()

    elif target == "elm":
        make_elm()

    else:
        print(f'Unknown target "{target}"')


def make_static():
    shutil.copytree("static", "build", dirs_exist_ok=True)


def make_style():
    cmd("npx sass", "--no-source-map", "style/style.scss", "build/style.css")


def make_elm():
    cmd("elm make", "src/Main.elm", "--optimize", "--output build/_tmp.js")

    with open("build/_tmp.js", "rt") as f:
        elm = f.read()

    with open("scripts/script.js", "rt") as f:
        base = f.read()

    with open("build/elm.js", "wt") as f:
        f.write(elm + base)


def cmd(command, *args):
    if os.system(" ".join([command] + [str(arg) for arg in args])):
        raise Exception(f"command {command} failed")


if __name__ == "__main__":
    main(sys.argv)
