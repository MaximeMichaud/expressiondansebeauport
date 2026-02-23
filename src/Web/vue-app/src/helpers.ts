export async function getFileFromUrl(url: string, name: string, defaultType = 'image/jpeg'): Promise<File> {
  const response = await fetch(url);
  const data = await response.blob();
  return new File([data], name, {
    type: data.type || defaultType,
  });
}