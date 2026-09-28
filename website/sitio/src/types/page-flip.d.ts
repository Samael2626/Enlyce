declare module "page-flip" {
  interface PageFlipSettings {
    readonly width: number;
    readonly height: number;
    readonly size?: "fixed" | "stretch";
    readonly minWidth?: number;
    readonly maxWidth?: number;
    readonly minHeight?: number;
    readonly maxHeight?: number;
    readonly drawShadow?: boolean;
    readonly flippingTime?: number;
    readonly usePortrait?: boolean;
    readonly autoSize?: boolean;
    readonly maxShadowOpacity?: number;
    readonly showCover?: boolean;
    readonly mobileScrollSupport?: boolean;
  }

  interface PageFlipEvent {
    readonly data: number | string;
  }

  export class PageFlip {
    constructor(element: HTMLElement, settings: PageFlipSettings);
    loadFromHTML(elements: HTMLElement[]): void;
    on(event: "flip", callback: (event: PageFlipEvent) => void): void;
    flipNext(): void;
    flipPrev(): void;
    destroy(): void;
  }
}
