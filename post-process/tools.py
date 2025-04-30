import cv2


def upscale_image(img, scale=4/3):
    """Upscale image with cubic interpolation"""
    new_size = (int(img.shape[1] * scale), int(img.shape[0] * scale))
    return cv2.resize(img, new_size, interpolation=cv2.INTER_CUBIC)

def upscale_mask(mask, scale=4/3):
    """Upscale mask with nearest-neighbor interpolation and binarize"""
    new_size = (int(mask.shape[1] * scale), int(mask.shape[0] * scale))
    upscaled = cv2.resize(mask, new_size, interpolation=cv2.INTER_NEAREST)
    _, binary_mask = cv2.threshold(upscaled, 1, 255, cv2.THRESH_BINARY)
    return binary_mask