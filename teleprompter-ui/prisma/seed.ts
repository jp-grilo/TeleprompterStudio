import { PrismaClient } from '@prisma/client'
const prisma = new PrismaClient()

async function main() {
  // Clear existing
  await prisma.songFolder.deleteMany()
  await prisma.song.deleteMany()
  await prisma.folder.deleteMany()

  // Músicas neutras / Pop Rock
  const song1 = await prisma.song.create({
    data: {
      title: 'Bohemian Rhapsody',
      artist: 'Queen',
      album: 'A Night at the Opera',
      isFavorite: true,
      rawContent: `Is this the real life?
Is this just fantasy?
Caught in a landslide,
No escape from reality.
Open your eyes,
Look up to the skies and see...`,
    },
  })

  const song2 = await prisma.song.create({
    data: {
      title: 'Hotel California',
      artist: 'Eagles',
      album: 'Hotel California',
      isFavorite: true,
      rawContent: `On a dark desert highway,
Cool wind in my hair
Warm smell of colitas,
Rising up through the air
Up ahead in the distance,
I saw a shimmering light...`,
    },
  })

  const song3 = await prisma.song.create({
    data: {
      title: 'Imagine',
      artist: 'John Lennon',
      album: 'Imagine',
      isFavorite: false,
      rawContent: `Imagine there's no heaven
It's easy if you try
No hell below us
Above us, only sky
Imagine all the people
Livin' for today...`,
    },
  })

  const song4 = await prisma.song.create({
    data: {
      title: 'Wonderwall',
      artist: 'Oasis',
      album: 'What\'s the Story Morning Glory',
      isFavorite: false,
      rawContent: `Today is gonna be the day
That they're gonna throw it back to you
By now you should've somehow
Realized what you gotta do
I don't believe that anybody
Feels the way I do about you now...`,
    },
  })

  // Pastas
  const rockFolder = await prisma.folder.create({
    data: {
      name: 'Setlist Principal',
      icon: '🎸',
      orderIndex: 0,
    },
  })

  const acusticoFolder = await prisma.folder.create({
    data: {
      name: 'Acústico (Subpasta)',
      icon: '🪕',
      parentFolderId: rockFolder.id, // SUBPASTA
      orderIndex: 1,
    },
  })

  const anos90Folder = await prisma.folder.create({
    data: {
      name: 'Anos 90',
      icon: '💿',
      orderIndex: 1,
    },
  })

  // Associações
  await prisma.songFolder.create({
    data: { songId: song1.id, folderId: rockFolder.id, order: 0 },
  })
  await prisma.songFolder.create({
    data: { songId: song2.id, folderId: rockFolder.id, order: 1 },
  })
  
  await prisma.songFolder.create({
    data: { songId: song3.id, folderId: acusticoFolder.id, order: 0 },
  })

  await prisma.songFolder.create({
    data: { songId: song4.id, folderId: anos90Folder.id, order: 0 },
  })

  console.log('Seed concluído com músicas clássicas e neutras, incluindo subpastas!')
}

main()
  .catch((e) => {
    console.error(e)
    process.exit(1)
  })
  .finally(async () => {
    await prisma.$disconnect()
  })
