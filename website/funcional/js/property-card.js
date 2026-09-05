import {
  formatCurrency,
  formatLocation,
  formatPropertyFacts,
  formatPublishedAt,
  safeMediaUrl,
} from './format.js';
import { updateFavoriteButton } from './favorites.js';

export function toPropertyCardModel(property) {
  const features = property.features || property;
  const coverPhoto = property.coverPhoto
    || property.photos?.find((photo) => photo.isCover)
    || property.photos?.[0]
    || null;

  return {
    ...property,
    coverPhoto,
    areaSquareMeters: features.areaSquareMeters,
    bedrooms: features.bedrooms,
    bathrooms: features.bathrooms,
    parkingSpaces: features.parkingSpaces,
  };
}

export function createPropertyCard({ property, template, href, favorites, onFavoriteChange }) {
  const cardProperty = toPropertyCardModel(property);
  const card = template.content.firstElementChild.cloneNode(true);
  for (const link of card.querySelectorAll('[data-property-link]')) link.href = href;

  card.querySelector('[data-operation]').textContent = cardProperty.operation;
  card.querySelector('[data-location]').textContent = formatLocation(cardProperty.location);
  card.querySelector('[data-title]').textContent = cardProperty.publicTitle;
  card.querySelector('[data-price]').textContent = formatCurrency(cardProperty.price);
  card.querySelector('[data-facts]').textContent = formatPropertyFacts(cardProperty);
  card.querySelector('[data-published]').textContent = `Publicado ${formatPublishedAt(cardProperty.publishedAt)}`;

  const favoriteButton = card.querySelector('[data-favorite]');
  updateFavoriteButton(favoriteButton, favorites.has(property.slug));
  favoriteButton.addEventListener('click', () => {
    const result = favorites.toggle(property.slug);
    updateFavoriteButton(favoriteButton, result.isFavorite);
    onFavoriteChange?.(result);
  });

  const image = card.querySelector('[data-property-image]');
  const fallback = card.querySelector('[data-image-fallback]');
  const imageUrl = safeMediaUrl(cardProperty.coverPhoto?.url);
  if (imageUrl) {
    image.src = imageUrl;
    image.alt = cardProperty.coverPhoto.altText || cardProperty.publicTitle;
    fallback.hidden = true;
    image.addEventListener('error', () => {
      image.hidden = true;
      fallback.hidden = false;
    }, { once: true });
  } else {
    image.hidden = true;
  }

  return card;
}
