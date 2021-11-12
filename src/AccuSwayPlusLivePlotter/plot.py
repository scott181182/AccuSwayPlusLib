import fileinput
import sys

import matplotlib.pyplot as plt
import numpy as np



def get_cop(fz, mx, my):
    return (my / fz, mx / fz)


def main():
    plt.ion()
    fig, ax = plt.subplots(1, 1, figsize=(6,6))
    px, py = [ 0 ], [ 0 ]
    sc = ax.scatter(px, py)
    ax.set_xlim(-25, 25)
    ax.set_ylim(-25, 25)
    ax.grid(True)
    plt.pause(0.5)
    plt.draw()
    plt.pause(0.5)

    for line in fileinput.input():
        lp = line.strip()
        print(lp)
        if line == "zero": print("Zeroed!")
        if line == "stop": break
        if not line.startswith("("): continue
        fx, fy, fz, mx, my, mz = list(map(lambda n: float(n), lp[1:-1].split(",")))
        cx, cy = get_cop(fz, mx, my)
        px[0] = cx
        py[0] = cy
        sc.set_offsets(np.c_[px, py])
        fig.canvas.draw_idle()
        plt.pause(0.001)
    plt.close()


if __name__ == "__main__":
    main()